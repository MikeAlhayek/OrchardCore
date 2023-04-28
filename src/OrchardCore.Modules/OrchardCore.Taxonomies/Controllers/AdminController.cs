using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Modules;
using OrchardCore.Routing;
using OrchardCore.Security.Permissions;
using OrchardCore.Taxonomies.Models;
using YesSql;

namespace OrchardCore.Taxonomies.Controllers
{
    public class AdminController : Controller
    {
        private readonly IContentManager _contentManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly IContentItemDisplayManager _contentItemDisplayManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ISession _session;
        private readonly IHtmlLocalizer H;
        private readonly INotifier _notifier;
        private readonly IUpdateModelAccessor _updateModelAccessor;
        private readonly ILogger _logger;
        private readonly IEnumerable<IContentHandler> _contentHandlers;
        private readonly IClock _clock;

        public AdminController(
            ISession session,
            IContentManager contentManager,
            IAuthorizationService authorizationService,
            IContentItemDisplayManager contentItemDisplayManager,
            IContentDefinitionManager contentDefinitionManager,
            INotifier notifier,
            IHtmlLocalizer<AdminController> localizer,
            IUpdateModelAccessor updateModelAccessor,
            ILogger<AdminController> logger,
            IEnumerable<IContentHandler> contentHandlers,
            IClock clock)
        {
            _contentManager = contentManager;
            _authorizationService = authorizationService;
            _contentItemDisplayManager = contentItemDisplayManager;
            _contentDefinitionManager = contentDefinitionManager;
            _session = session;
            _notifier = notifier;
            _updateModelAccessor = updateModelAccessor;
            _logger = logger;
            _contentHandlers = contentHandlers;
            _clock = clock;
            H = localizer;
        }

        public async Task<IActionResult> Create(string id, string taxonomyContentItemId, string taxonomyItemId)
        {
            if (String.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            if (_contentDefinitionManager.GetTypeDefinition(id) == null)
            {
                return NotFound();
            }

            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageTaxonomies))
            {
                return Forbid();
            }

            var contentItem = await _contentManager.NewAsync(id);
            contentItem.Weld<TermPart>();
            contentItem.Alter<TermPart>(t => t.TaxonomyContentItemId = taxonomyContentItemId);

            dynamic model = await _contentItemDisplayManager.BuildEditorAsync(contentItem, _updateModelAccessor.ModelUpdater, true);

            model.TaxonomyContentItemId = taxonomyContentItemId;
            model.TaxonomyItemId = taxonomyItemId;

            return View(model);
        }

        [HttpPost]
        [ActionName("Create")]
        [FormValueRequired("submit.Save")]
        public async Task<IActionResult> CreatePost(string id, string taxonomyContentItemId, string taxonomyItemId, [Bind(Prefix = "submit.Save")] string submitSave, string returnUrl)
        {
            if (String.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            if (_contentDefinitionManager.GetTypeDefinition(id) == null)
            {
                return NotFound();
            }

            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageTaxonomies))
            {
                return Forbid();
            }

            var stayOnSamePage = submitSave == "submit.SaveAndContinue";

            return await CreatePostAsync(id, taxonomyContentItemId, taxonomyItemId, returnUrl, stayOnSamePage, false, async (item, typeDefinition) =>
            {
                await _notifier.SuccessAsync(String.IsNullOrWhiteSpace(typeDefinition.DisplayName)
                    ? H["Your content draft has been saved."]
                    : H["Your {0} draft has been saved.", typeDefinition.DisplayName]);
            });
        }

        [HttpPost]
        [ActionName("Create")]
        [FormValueRequired("submit.Publish")]
        public async Task<IActionResult> CreateAndPublishPost(string id, string taxonomyContentItemId, string taxonomyItemId, [Bind(Prefix = "submit.Publish")] string submitPublish, string returnUrl)
        {
            if (String.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            if (_contentDefinitionManager.GetTypeDefinition(id) == null)
            {
                return NotFound();
            }

            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageTaxonomies))
            {
                return Forbid();
            }

            var stayOnSamePage = submitPublish == "submit.PublishAndContinue";

            return await CreatePostAsync(id, taxonomyContentItemId, taxonomyItemId, returnUrl, stayOnSamePage, true, async (item, typeDefinition) =>
            {
                await _notifier.SuccessAsync(String.IsNullOrWhiteSpace(typeDefinition.DisplayName)
                        ? H["Your content has been published."]
                        : H["Your {0} has been published.", typeDefinition.DisplayName]);
            });
        }

        public async Task<IActionResult> Edit(string taxonomyContentItemId, string taxonomyItemId)
        {
            if (String.IsNullOrWhiteSpace(taxonomyContentItemId) || String.IsNullOrWhiteSpace(taxonomyItemId))
            {
                return NotFound();
            }

            var taxonomy = await _contentManager.GetAsync(taxonomyContentItemId, VersionOptions.Latest);

            if (taxonomy == null)
            {
                return NotFound();
            }

            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageTaxonomies, taxonomy))
            {
                return Forbid();
            }

            // Look for the target taxonomy item in the hierarchy
            JObject taxonomyItem = FindTaxonomyItem(taxonomy.As<TaxonomyPart>().Content, taxonomyItemId);

            // Couldn't find targeted taxonomy item
            if (taxonomyItem == null)
            {
                return NotFound();
            }

            var contentItem = taxonomyItem.ToObject<ContentItem>();
            contentItem.Weld<TermPart>();
            contentItem.Alter<TermPart>(t => t.TaxonomyContentItemId = taxonomyContentItemId);

            dynamic model = await _contentItemDisplayManager.BuildEditorAsync(contentItem, _updateModelAccessor.ModelUpdater, false);

            model.TaxonomyContentItemId = taxonomyContentItemId;
            model.TaxonomyItemId = taxonomyItemId;

            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("submit.Save")]
        public Task<IActionResult> EditPOST(string taxonomyContentItemId, string taxonomyItemId, [Bind(Prefix = "submit.Save")] string submitSave, string returnUrl)
        {
            var stayOnSamePage = submitSave == "submit.SaveAndContinue";
            return EditPostAsync(taxonomyContentItemId, taxonomyItemId, returnUrl, stayOnSamePage, false, async (contentItem, typeDefinition) =>
            {
                await _notifier.SuccessAsync(String.IsNullOrWhiteSpace(typeDefinition.DisplayName)
                    ? H["Your content draft has been saved."]
                    : H["Your {0} draft has been saved.", typeDefinition.DisplayName]);
            });
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("submit.Publish")]
        public Task<IActionResult> EditAndPublishPOST(string taxonomyContentItemId, string taxonomyItemId, [Bind(Prefix = "submit.Publish")] string submitPublish, string returnUrl)
        {
            var stayOnSamePage = submitPublish == "submit.PublishAndContinue";

            return EditPostAsync(taxonomyContentItemId, taxonomyItemId, returnUrl, stayOnSamePage, true, async (contentItem, typeDefinition) =>
            {
                await _notifier.SuccessAsync(String.IsNullOrWhiteSpace(typeDefinition.DisplayName)
                    ? H["Your content has been published."]
                    : H["Your {0} has been published.", typeDefinition.DisplayName]);
            });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string taxonomyContentItemId, string taxonomyItemId)
        {
            if (String.IsNullOrWhiteSpace(taxonomyContentItemId) || String.IsNullOrWhiteSpace(taxonomyItemId))
            {
                return NotFound();
            }

            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageTaxonomies))
            {
                return Forbid();
            }

            ContentItem taxonomy;

            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition("Taxonomy");

            if (!contentTypeDefinition.IsDraftable())
            {
                taxonomy = await _contentManager.GetAsync(taxonomyContentItemId, VersionOptions.Latest);
            }
            else
            {
                taxonomy = await _contentManager.GetAsync(taxonomyContentItemId, VersionOptions.DraftRequired);
            }

            if (taxonomy == null)
            {
                return NotFound();
            }

            // Look for the target taxonomy item in the hierarchy
            var taxonomyItem = FindTaxonomyItem(taxonomy.As<TaxonomyPart>().Content, taxonomyItemId);

            // Couldn't find targeted taxonomy item
            if (taxonomyItem == null)
            {
                return NotFound();
            }

            taxonomyItem.Remove();
            _session.Save(taxonomy);

            await _notifier.SuccessAsync(H["Taxonomy item deleted successfully."]);

            return RedirectToAction(nameof(Edit), "Admin", new { area = "OrchardCore.Contents", contentItemId = taxonomyContentItemId });
        }

        private async Task<IActionResult> CreatePostAsync(
            string id,
            string taxonomyContentItemId,
            string taxonomyItemId,
            string returnUrl,
            bool stayOnSamePage,
            bool shouldPublish,
            Func<ContentItem, ContentTypeDefinition, Task> notify)
        {
            ContentItem taxonomy;

            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition("Taxonomy");

            if (!contentTypeDefinition.IsDraftable())
            {
                taxonomy = await _contentManager.GetAsync(taxonomyContentItemId, VersionOptions.Latest);
            }
            else
            {
                taxonomy = await _contentManager.GetAsync(taxonomyContentItemId, VersionOptions.DraftRequired);
            }

            if (taxonomy == null)
            {
                return NotFound();
            }

            var termContentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(id);

            if (termContentTypeDefinition == null)
            {
                return NotFound();
            }

            var contentItem = await CreateContentItemOwnedByCurrentUserAsync(id);
            contentItem.Weld<TermPart>();
            contentItem.Alter<TermPart>(t => t.TaxonomyContentItemId = taxonomyContentItemId);

            dynamic model = await _contentItemDisplayManager.UpdateEditorAsync(contentItem, _updateModelAccessor.ModelUpdater, true);

            if (!ModelState.IsValid)
            {
                model.TaxonomyContentItemId = taxonomyContentItemId;
                model.TaxonomyItemId = taxonomyItemId;

                return View(model);
            }

            var context = new CreateContentContext(contentItem);

            await _contentHandlers.InvokeAsync((handler, context) => handler.CreatingAsync(context), context, _logger);
            await _contentHandlers.InvokeAsync((handler, context) => handler.CreatedAsync(context), context, _logger);

            if (termContentTypeDefinition.IsDraftable() && shouldPublish)
            {
                contentItem.Published = true;

                var publishContext = new PublishContentContext(contentItem, null);

                await _contentHandlers.InvokeAsync((handler, context) => handler.PublishingAsync(context), publishContext, _logger);
                await _contentHandlers.InvokeAsync((handler, context) => handler.PublishedAsync(context), publishContext, _logger);
            }
            else
            {
                contentItem.Published = false;
                var draftContext = new SaveDraftContentContext(contentItem);

                await _contentHandlers.InvokeAsync((handler, context) => handler.DraftSavingAsync(context), draftContext, _logger);
                await _contentHandlers.InvokeAsync((handler, context) => handler.DraftSavedAsync(context), draftContext, _logger);
            }

            if (taxonomyItemId == null)
            {
                // Use the taxonomy as the parent if no target is specified
                taxonomy.Alter<TaxonomyPart>(part => part.Terms.Add(contentItem));
            }
            else
            {
                // Look for the target taxonomy item in the hierarchy
                var parentTaxonomyItem = FindTaxonomyItem(taxonomy.As<TaxonomyPart>().Content, taxonomyItemId);

                // Couldn't find targeted taxonomy item
                if (parentTaxonomyItem == null)
                {
                    return NotFound();
                }

                var taxonomyItems = parentTaxonomyItem?.Terms as JArray;

                if (taxonomyItems == null)
                {
                    parentTaxonomyItem["Terms"] = taxonomyItems = new JArray();
                }

                taxonomyItems.Add(JObject.FromObject(contentItem));
            }

            await notify(contentItem, termContentTypeDefinition);

            return RedirectAsync(taxonomyContentItemId, taxonomyItemId, returnUrl, stayOnSamePage);
        }

        private IActionResult RedirectAsync(string taxonomyContentItemId, string taxonomyItemId, string returnUrl, bool stayOnSamePage)
        {
            var hasReturnUrl = !String.IsNullOrEmpty(returnUrl);

            if (hasReturnUrl && !stayOnSamePage)
            {
                return this.LocalRedirect(returnUrl, true);
            }

            var adminRouteValues = new RouteValueDictionary()
            {
                { nameof(taxonomyContentItemId), taxonomyContentItemId },
                { nameof(taxonomyItemId), taxonomyItemId },
            };

            if (hasReturnUrl)
            {
                adminRouteValues.Add("returnUrl", returnUrl);
            }

            return RedirectToAction(nameof(Edit), adminRouteValues);
        }

        private async Task<ContentItem> CreateContentItemOwnedByCurrentUserAsync(string contentType)
        {
            var contentItem = await _contentManager.NewAsync(contentType);
            contentItem.Owner = CurrentUserId();

            return contentItem;
        }

        private string CurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        private async Task<bool> IsAuthorizedAsync(Permission permission)
        {
            return await _authorizationService.AuthorizeAsync(User, permission);
        }

        private async Task<bool> IsAuthorizedAsync(Permission permission, object resource)
        {
            return await _authorizationService.AuthorizeAsync(User, permission, resource);
        }

        private async Task<IActionResult> EditPostAsync(
            string taxonomyContentItemId,
            string taxonomyItemId,
            string returnUrl,
            bool stayOnSamePage,
            bool shouldPublish,
            Func<ContentItem, ContentTypeDefinition, Task> notify)
        {
            if (String.IsNullOrWhiteSpace(taxonomyContentItemId) || String.IsNullOrWhiteSpace(taxonomyItemId))
            {
                return NotFound();
            }

            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageTaxonomies))
            {
                return Forbid();
            }

            ContentItem taxonomy;

            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition("Taxonomy");

            if (!contentTypeDefinition.IsDraftable())
            {
                taxonomy = await _contentManager.GetAsync(taxonomyContentItemId, VersionOptions.Latest);
            }
            else
            {
                taxonomy = await _contentManager.GetAsync(taxonomyContentItemId, VersionOptions.DraftRequired);
            }

            if (taxonomy == null)
            {
                return NotFound();
            }

            // Look for the target taxonomy item in the hierarchy
            JObject taxonomyItem = FindTaxonomyItem(taxonomy.As<TaxonomyPart>().Content, taxonomyItemId);

            // Couldn't find targeted taxonomy item
            if (taxonomyItem == null)
            {
                return NotFound();
            }

            var existing = taxonomyItem.ToObject<ContentItem>();

            // Create a new item to take into account the current type definition.
            var contentItem = await _contentManager.NewAsync(existing.ContentType);

            contentItem.ContentItemId = existing.ContentItemId;
            contentItem.Merge(existing);
            contentItem.Weld<TermPart>();
            contentItem.Alter<TermPart>(t => t.TaxonomyContentItemId = taxonomyContentItemId);

            dynamic model = await _contentItemDisplayManager.UpdateEditorAsync(contentItem, _updateModelAccessor.ModelUpdater, false);

            if (!ModelState.IsValid)
            {
                model.TaxonomyContentItemId = taxonomyContentItemId;
                model.TaxonomyItemId = taxonomyItemId;

                return View(model);
            }

            var termContentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);

            var context = new UpdateContentContext(contentItem);

            await _contentHandlers.InvokeAsync((handler, context) => handler.UpdatingAsync(context), context, _logger);
            await _contentHandlers.InvokeAsync((handler, context) => handler.UpdatedAsync(context), context, _logger);

            if (shouldPublish && termContentTypeDefinition.IsDraftable())
            {
                contentItem.Published = true;

                var publishContext = new PublishContentContext(contentItem, null);

                await _contentHandlers.InvokeAsync((handler, context) => handler.PublishingAsync(context), publishContext, _logger);
                await _contentHandlers.InvokeAsync((handler, context) => handler.PublishedAsync(context), publishContext, _logger);
            }
            else
            {
                contentItem.Published = false;
                var draftContext = new SaveDraftContentContext(contentItem);

                await _contentHandlers.InvokeAsync((handler, context) => handler.DraftSavingAsync(context), draftContext, _logger);
                await _contentHandlers.InvokeAsync((handler, context) => handler.DraftSavedAsync(context), draftContext, _logger);
            }

            taxonomyItem.Merge(contentItem.Content, new JsonMergeSettings
            {
                MergeArrayHandling = MergeArrayHandling.Replace,
                MergeNullValueHandling = MergeNullValueHandling.Merge
            });

            if (taxonomyItem[nameof(ContentItem.Owner)] == null)
            {
                taxonomyItem[nameof(ContentItem.Owner)] = CurrentUserId();
            }

            // Merge doesn't copy the properties.
            taxonomyItem[nameof(ContentItem.DisplayText)] = contentItem.DisplayText;

            _session.Save(taxonomy);
            await notify(contentItem, termContentTypeDefinition);

            return RedirectAsync(taxonomyContentItemId, taxonomyItemId, returnUrl, stayOnSamePage);
        }

        private JObject FindTaxonomyItem(JObject contentItem, string taxonomyItemId)
        {
            if (contentItem["ContentItemId"]?.Value<string>() == taxonomyItemId)
            {
                return contentItem;
            }

            if (contentItem.GetValue("Terms") == null)
            {
                return null;
            }

            var taxonomyItems = (JArray)contentItem["Terms"];

            JObject result;

            foreach (JObject taxonomyItem in taxonomyItems)
            {
                // Search in inner taxonomy items.
                result = FindTaxonomyItem(taxonomyItem, taxonomyItemId);

                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }
    }
}
