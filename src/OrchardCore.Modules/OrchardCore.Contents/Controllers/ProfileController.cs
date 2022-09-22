using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Contents.Indexing;
using OrchardCore.Contents.Models;
using OrchardCore.Contents.Services;
using OrchardCore.Contents.ViewModels;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Routing;
using OrchardCore.Security.Permissions;
using YesSql;
using YesSql.Filters.Query;
using YesSql.Services;

namespace OrchardCore.Contents.Controllers;

[Feature("OrchardCore.Contents.Profile")]
[Admin]
public class ProfileController : Controller, IUpdateModel
{
    private readonly IContentManager _contentManager;
    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly PagerOptions _pagerOptions;
    private readonly ISession _session;
    private readonly IContentItemDisplayManager _contentItemDisplayManager;
    private readonly INotifier _notifier;
    private readonly IAuthorizationService _authorizationService;
    private readonly IDisplayManager<ContentOptionsViewModel> _contentOptionsDisplayManager;
    private readonly IContentsAdminListQueryService _contentsAdminListQueryService;
    private readonly IHtmlLocalizer H;
    private readonly IStringLocalizer S;
    private readonly IUpdateModelAccessor _updateModelAccessor;
    private readonly IShapeFactory _shapeFactory;
    private readonly dynamic New;
    private readonly ILogger _logger;

    public ProfileController(
        IAuthorizationService authorizationService,
        IContentManager contentManager,
        IContentItemDisplayManager contentItemDisplayManager,
        IContentDefinitionManager contentDefinitionManager,
        IOptions<PagerOptions> pagerOptions,
        INotifier notifier,
        ISession session,
        IShapeFactory shapeFactory,
        IDisplayManager<ContentOptionsViewModel> contentOptionsDisplayManager,
        IContentsAdminListQueryService contentsAdminListQueryService,
        ILogger<ProfileController> logger,
        IHtmlLocalizer<ProfileController> htmlLocalizer,
        IStringLocalizer<ProfileController> stringLocalizer,
        IUpdateModelAccessor updateModelAccessor)
    {
        _authorizationService = authorizationService;
        _notifier = notifier;
        _contentItemDisplayManager = contentItemDisplayManager;
        _session = session;
        _pagerOptions = pagerOptions.Value;
        _contentManager = contentManager;
        _contentDefinitionManager = contentDefinitionManager;
        _updateModelAccessor = updateModelAccessor;
        _contentOptionsDisplayManager = contentOptionsDisplayManager;
        _contentsAdminListQueryService = contentsAdminListQueryService;

        H = htmlLocalizer;
        S = stringLocalizer;
        _shapeFactory = shapeFactory;
        New = shapeFactory;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> List(string profileId, string contentTypeId,
        [ModelBinder(BinderType = typeof(ContentItemFilterEngineModelBinder), Name = "q")] QueryFilterResult<ContentItem> queryFilterResult,
        ContentOptionsViewModel options,
        PagerParameters pagerParameters)
    {
        if (String.IsNullOrWhiteSpace(profileId) || String.IsNullOrWhiteSpace(contentTypeId))
        {
            return NotFound();
        }

        // TODO, ensure contentTypeDefinition belongs to a profile
        var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(contentTypeId);

        if (contentTypeDefinition == null || !contentTypeDefinition.GetSettings<ContentTypeSettings>().Listable)
        {
            return NotFound();
        }

        if (!await _authorizationService.AuthorizeContentTypeDefinitionsAsync(User, CommonPermissions.ViewContent, new[] { contentTypeDefinition }, _contentManager))
        {
            return Forbid();
        }

        var profileContentItem = await _contentManager.GetAsync(profileId);

        if (profileContentItem == null)
        {
            return NotFound();
        }

        if (!await IsAuthorizedAsync(CommonPermissions.ViewContent, profileContentItem))
        {
            return Forbid();
        }

        options.SelectedContentType = contentTypeId;

        // The filter is bound seperately and mapped to the options.
        // The options must still be bound so that options that are not filters are still bound
        options.FilterResult = queryFilterResult;

        // When the selected content type is provided via the route or options a placeholder node is used to apply a filter.
        options.FilterResult.TryAddOrReplace(new ContentTypeFilterNode(options.SelectedContentType));

        options.CreatableTypes = new List<SelectListItem>();

        // Allows non creatable types to be created by another admin page.
        if (contentTypeDefinition.IsCreatable() || options.CanCreateSelectedContentType)
        {
            var contentItem = await CreateContentItemForOwnedByCurrentAsync(contentTypeDefinition.Name, profileId);

            if (await IsAuthorizedAsync(CommonPermissions.EditContent, contentItem))
            {
                options.CreatableTypes.Add(new SelectListItem(contentTypeDefinition.DisplayName, contentTypeDefinition.Name));
            }
        }

        // We populate the remaining SelectLists.
        options.ContentStatuses = new List<SelectListItem>()
        {
            new SelectListItem() { Text = S["Latest"], Value = nameof(ContentsStatus.Latest), Selected = options.ContentsStatus == ContentsStatus.Latest },
            new SelectListItem() { Text = S["Published"], Value = nameof(ContentsStatus.Published), Selected = options.ContentsStatus == ContentsStatus.Published },
            new SelectListItem() { Text = S["Unpublished"], Value = nameof(ContentsStatus.Draft), Selected = options.ContentsStatus == ContentsStatus.Draft },
            new SelectListItem() { Text = S["All versions"], Value = nameof(ContentsStatus.AllVersions), Selected = options.ContentsStatus == ContentsStatus.AllVersions }
        };

        if (await IsAuthorizedAsync(Permissions.ListContent))
        {
            options.ContentStatuses.Insert(1, new SelectListItem() { Text = S["Owned by me"], Value = nameof(ContentsStatus.Owner) });
        }

        options.ContentSorts = new List<SelectListItem>()
        {
            new SelectListItem() { Text = S["Recently created"], Value = nameof(ContentsOrder.Created), Selected = options.OrderBy == ContentsOrder.Created },
            new SelectListItem() { Text = S["Recently modified"], Value = nameof(ContentsOrder.Modified), Selected = options.OrderBy == ContentsOrder.Modified },
            new SelectListItem() { Text = S["Recently published"], Value = nameof(ContentsOrder.Published), Selected = options.OrderBy == ContentsOrder.Published },
            new SelectListItem() { Text = S["Title"], Value = nameof(ContentsOrder.Title), Selected = options.OrderBy == ContentsOrder.Title },
        };

        options.ContentsBulkAction = new List<SelectListItem>()
        {
            new SelectListItem() { Text = S["Publish Now"], Value = nameof(ContentsBulkAction.PublishNow) },
            new SelectListItem() { Text = S["Unpublish"], Value = nameof(ContentsBulkAction.Unpublish) },
            new SelectListItem() { Text = S["Delete"], Value = nameof(ContentsBulkAction.Remove) }
        };

        // If ContentTypeOptions is not initialized by query string or by the code above, initialize it
        if (options.ContentTypeOptions == null)
        {
            options.ContentTypeOptions = new List<SelectListItem>();
        }

        // With the options populated we filter the query, allowing the filters to alter the options.
        var query = await _contentsAdminListQueryService.QueryAsync(options, _updateModelAccessor.ModelUpdater);

        query.With<ContainedProfilePartIndex>(x => x.ProfileContentItemId == profileId);

        // The search text is provided back to the UI.
        options.SearchText = options.FilterResult.ToString();
        options.OriginalSearchText = options.SearchText;

        // Populate route values to maintain previous route data when generating page links.
        options.RouteValues.TryAdd("q", options.FilterResult.ToString());

        var pager = new Pager(pagerParameters, _pagerOptions.GetPageSize());

        var routeData = new RouteData(options.RouteValues);

        var pagerShape = (await New.Pager(pager)).TotalItemCount(_pagerOptions.MaxPagedCount > 0 ? _pagerOptions.MaxPagedCount : await query.CountAsync()).RouteData(routeData);

        // Load items so that loading handlers are invoked.
        var pageOfContentItems = await query.Skip(pager.GetStartIndex()).Take(pager.PageSize).ListAsync(_contentManager);

        // We prepare the content items SummaryAdmin shape
        var contentItemSummaries = new List<dynamic>();
        foreach (var contentItem in pageOfContentItems)
        {
            contentItemSummaries.Add(await _contentItemDisplayManager.BuildDisplayAsync(contentItem, _updateModelAccessor.ModelUpdater, "SummaryAdmin"));
        }

        // Populate options pager summary values.
        var startIndex = ((pagerShape.Page - 1) * pagerShape.PageSize) + 1;
        options.StartIndex = startIndex;
        options.EndIndex = startIndex + contentItemSummaries.Count - 1;
        options.ContentItemsCount = contentItemSummaries.Count;
        options.TotalItemCount = pagerShape.TotalItemCount;

        var model = await GetProfileShapeAync(profileContentItem, contentTypeDefinition, null, await _shapeFactory.CreateAsync<ListContentsViewModel>("ContentsAdminList", async viewModel =>
        {
            viewModel.ContentItems = contentItemSummaries;
            viewModel.Pager = pagerShape;
            viewModel.Options = options;
            viewModel.Header = await _contentOptionsDisplayManager.BuildEditorAsync(options, _updateModelAccessor.ModelUpdater, false);
        }));

        return View(model);
    }

    [HttpPost, ActionName("List")]
    [FormValueRequired("submit.Filter")]
    public async Task<ActionResult> ListFilterPOST(ContentOptionsViewModel options)
    {
        // When the user has typed something into the search input no further evaluation of the form post is required.
        if (!String.Equals(options.SearchText, options.OriginalSearchText, StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(List), new RouteValueDictionary { { "q", options.SearchText } });
        }

        // Evaluate the values provided in the form post and map them to the filter result and route values.
        await _contentOptionsDisplayManager.UpdateEditorAsync(options, _updateModelAccessor.ModelUpdater, false);

        // The route value must always be added after the editors have updated the models.
        options.RouteValues.TryAdd("q", options.FilterResult.ToString());

        return RedirectToAction(nameof(List), options.RouteValues);
    }

    [HttpPost, ActionName("List")]
    [FormValueRequired("submit.BulkAction")]
    public async Task<ActionResult> ListPOST(ContentOptionsViewModel options, IEnumerable<int> itemIds)
    {
        if (itemIds?.Count() > 0)
        {
            // Load items so that loading handlers are invoked.
            var checkedContentItems = await _session.Query<ContentItem, ContentItemIndex>().Where(x => x.DocumentId.IsIn(itemIds) && x.Latest).ListAsync(_contentManager);
            switch (options.BulkAction)
            {
                case ContentsBulkAction.None:
                    break;
                case ContentsBulkAction.PublishNow:
                    foreach (var item in checkedContentItems)
                    {
                        if (!await _authorizationService.AuthorizeAsync(User, CommonPermissions.PublishContent, item))
                        {
                            await _notifier.WarningAsync(H["Couldn't publish selected content."]);
                            await _session.CancelAsync();
                            return Forbid();
                        }

                        await _contentManager.PublishAsync(item);
                    }
                    await _notifier.SuccessAsync(H["Content published successfully."]);
                    break;
                case ContentsBulkAction.Unpublish:
                    foreach (var item in checkedContentItems)
                    {
                        if (!await IsAuthorizedAsync(CommonPermissions.PublishContent, item))
                        {
                            await _notifier.WarningAsync(H["Couldn't unpublish selected content."]);
                            await _session.CancelAsync();
                            return Forbid();
                        }

                        await _contentManager.UnpublishAsync(item);
                    }
                    await _notifier.SuccessAsync(H["Content unpublished successfully."]);
                    break;
                case ContentsBulkAction.Remove:
                    foreach (var item in checkedContentItems)
                    {
                        if (!await IsAuthorizedAsync(CommonPermissions.DeleteContent, item))
                        {
                            await _notifier.WarningAsync(H["Couldn't remove selected content."]);
                            await _session.CancelAsync();
                            return Forbid();
                        }

                        await _contentManager.RemoveAsync(item);
                    }
                    await _notifier.SuccessAsync(H["Content removed successfully."]);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(options.BulkAction));
            }
        }

        return RedirectToAction(nameof(List));
    }

    public async Task<IActionResult> Create(string profileId, string contentTypeId)
    {
        if (String.IsNullOrWhiteSpace(profileId) || String.IsNullOrWhiteSpace(contentTypeId))
        {
            return NotFound();
        }

        var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(contentTypeId);

        if (contentTypeDefinition == null)
        {
            // given an invalid contentTypeId
            return NotFound();
        }

        var profileContentItem = await _contentManager.GetAsync(profileId);

        if (profileContentItem == null)
        {
            // given an invalid profileId
            return NotFound();
        }

        var profileTypeDefinition = _contentDefinitionManager.GetTypeDefinition(profileContentItem.ContentType);

        if (profileTypeDefinition == null)
        {
            // the given profileId does not have content definition
            return NotFound();
        }

        var profileSettings = profileTypeDefinition.GetSettings<ContentProfileSettings>();

        if (profileSettings.ContainedContentTypes == null || !profileSettings.ContainedContentTypes.Contains(contentTypeDefinition.Name, StringComparer.OrdinalIgnoreCase))
        {
            // the given contentTypeId does not belong to the profile
            return NotFound();
        }

        if (!await IsAuthorizedAsync(CommonPermissions.ViewContent, profileContentItem))
        {
            // no permission to view profile
            return Forbid();
        }

        var contentItem = await CreateContentItemForOwnedByCurrentAsync(contentTypeId, profileId);

        if (!await IsAuthorizedAsync(CommonPermissions.EditContent, contentItem))
        {
            return Forbid();
        }

        var model = await GetProfileShapeAync(profileContentItem, contentTypeDefinition, null, await _contentItemDisplayManager.BuildEditorAsync(contentItem, _updateModelAccessor.ModelUpdater, true));

        return View(model);
    }

    [HttpPost, ActionName("Create")]
    [FormValueRequired("submit.Save")]
    public async Task<IActionResult> CreatePOST(string profileId, string contentTypeId, [Bind(Prefix = "submit.Save")] string submitSave, string returnUrl)
    {
        var stayOnSamePage = submitSave == "submit.SaveAndContinue";

        var dummyContent = await CreateContentItemForOwnedByCurrentAsync(contentTypeId, profileId);

        if (!await IsAuthorizedAsync(CommonPermissions.PublishContent, dummyContent))
        {
            return Forbid();
        }

        return await CreatePOST(profileId, contentTypeId, returnUrl, stayOnSamePage, async contentItem =>
        {
            await _contentManager.SaveDraftAsync(contentItem);

            var typeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);

            await _notifier.SuccessAsync(String.IsNullOrWhiteSpace(typeDefinition.DisplayName)
                ? H["Your content draft has been saved."]
                : H["Your {0} draft has been saved.", typeDefinition.DisplayName]);
        });
    }

    [HttpPost, ActionName("Create")]
    [FormValueRequired("submit.Publish")]
    public async Task<IActionResult> CreateAndPublishPOST(string profileId, string contentTypeId, [Bind(Prefix = "submit.Publish")] string submitPublish, string returnUrl)
    {
        var stayOnSamePage = submitPublish == "submit.PublishAndContinue";
        // pass a dummy content to the authorizer to check for "own" variations
        var dummyContent = await CreateContentItemForOwnedByCurrentAsync(contentTypeId, profileId);

        if (!await IsAuthorizedAsync(CommonPermissions.PublishContent, dummyContent))
        {
            return Forbid();
        }

        return await CreatePOST(profileId, contentTypeId, returnUrl, stayOnSamePage, async contentItem =>
        {
            await _contentManager.PublishAsync(contentItem);

            var typeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);

            await _notifier.SuccessAsync(String.IsNullOrWhiteSpace(typeDefinition.DisplayName)
                ? H["Your content has been published."]
                : H["Your {0} has been published.", typeDefinition.DisplayName]);
        });
    }


    public async Task<IActionResult> Display(string profileId, string contentItemId)
    {
        if (String.IsNullOrWhiteSpace(profileId))
        {
            return NotFound();
        }

        var profileContentItem = await _contentManager.GetAsync(profileId);

        if (profileContentItem == null)
        {
            return NotFound();
        }

        if (!await IsAuthorizedAsync(CommonPermissions.ViewContent, profileContentItem))
        {
            return Forbid();
        }

        IShape shape;

        if (String.IsNullOrWhiteSpace(contentItemId))
        {
            // When contentItemId is empty, means we are displaying the profile not a content within a profile

            var profileTypeDefinition = _contentDefinitionManager.GetTypeDefinition(profileContentItem.ContentType);

            shape = await GetProfileShapeAync(profileContentItem, profileTypeDefinition, null, await _contentItemDisplayManager.BuildDisplayAsync(profileContentItem, _updateModelAccessor.ModelUpdater, "Detail"));
        }
        else
        {
            var contentItem = await _contentManager.GetAsync(contentItemId, VersionOptions.Latest);

            if (contentItem == null)
            {
                return NotFound();
            }

            if (!await IsAuthorizedAsync(CommonPermissions.ViewContent, contentItem))
            {
                return Forbid();
            }

            var profilePart = contentItem.As<ContainedProfilePart>();

            if (String.IsNullOrEmpty(profilePart?.ProfileContentItemId))
            {
                // At this point, this content item was not created using profile.
                // Or, it was created prior the content-type was configured as profile.
                // Redirect to standard content item 
                return RedirectToAction("Display", "Admin", new { contentItemId });
            }

            if (profilePart.ProfileContentItemId != profileId)
            {
                // if the contained ProfileId does not equal to profile, the given contentItemId does not belong
                // to the given profileId
                return NotFound();
            }

            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);

            shape = await GetProfileShapeAync(profileContentItem, contentTypeDefinition, contentItem, await _contentItemDisplayManager.BuildDisplayAsync(contentItem, _updateModelAccessor.ModelUpdater, "DetailAdmin"));
        }

        return View(shape);
    }

    public async Task<IActionResult> Edit(string profileId, string contentItemId)
    {
        if (String.IsNullOrWhiteSpace(profileId))
        {
            return NotFound();
        }

        var profileContentItem = await _contentManager.GetAsync(profileId);

        if (profileContentItem == null)
        {
            return NotFound();
        }

        if (!await IsAuthorizedAsync(CommonPermissions.ViewContent, profileContentItem))
        {
            return Forbid();
        }

        IShape shape;

        if (String.IsNullOrWhiteSpace(contentItemId))
        {
            // When contentItemId is empty, means we are displaying the profile not a content within a profile

            var profileTypeDefinition = _contentDefinitionManager.GetTypeDefinition(profileContentItem.ContentType);

            shape = await GetProfileShapeAync(profileContentItem, profileTypeDefinition, null, await _contentItemDisplayManager.BuildEditorAsync(profileContentItem, _updateModelAccessor.ModelUpdater, false));
        }
        else
        {
            var contentItem = await _contentManager.GetAsync(contentItemId, VersionOptions.Latest);

            if (contentItem == null)
            {
                return NotFound();
            }

            if (!await IsAuthorizedAsync(CommonPermissions.ViewContent, contentItem))
            {
                return Forbid();
            }

            var profilePart = contentItem.As<ContainedProfilePart>();

            if (String.IsNullOrEmpty(profilePart?.ProfileContentItemId))
            {
                // At this point, this content item was not created using profile.
                // Or, it was created prior the content-type was configured as profile.
                // Redirect to standard content item 
                return RedirectToAction("Edit", "Admin", new { contentItemId });
            }

            if (profilePart.ProfileContentItemId != profileId)
            {
                // if the contained ProfileId does not equal to profile, the given contentItemId does not belong
                // to the given profileId
                return NotFound();
            }

            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);

            shape = await GetProfileShapeAync(profileContentItem, contentTypeDefinition, contentItem, await _contentItemDisplayManager.BuildEditorAsync(contentItem, _updateModelAccessor.ModelUpdater, false));
        }

        return View(shape);
    }

    [HttpPost, ActionName("Edit")]
    [FormValueRequired("submit.Save")]
    public Task<IActionResult> EditPOST(string profileId, string contentItemId, [Bind(Prefix = "submit.Save")] string submitSave, string returnUrl)
    {
        var stayOnSamePage = submitSave == "submit.SaveAndContinue";
        return EditPOST(profileId, contentItemId, returnUrl, stayOnSamePage, async contentItem =>
        {
            await _contentManager.SaveDraftAsync(contentItem);

            var typeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);

            await _notifier.SuccessAsync(string.IsNullOrWhiteSpace(typeDefinition.DisplayName)
                ? H["Your content draft has been saved."]
                : H["Your {0} draft has been saved.", typeDefinition.DisplayName]);
        });
    }

    [HttpPost, ActionName("Edit")]
    [FormValueRequired("submit.Publish")]
    public async Task<IActionResult> EditAndPublishPOST(string profileId, string contentItemId, [Bind(Prefix = "submit.Publish")] string submitPublish, string returnUrl)
    {
        var stayOnSamePage = submitPublish == "submit.PublishAndContinue";

        var contentItem = await _contentManager.GetAsync(contentItemId, VersionOptions.Latest);

        if (contentItem == null)
        {
            return NotFound();
        }

        if (!await IsAuthorizedAsync(CommonPermissions.PublishContent, contentItem))
        {
            return Forbid();
        }

        return await EditPOST(profileId, contentItemId, returnUrl, stayOnSamePage, async contentItem =>
        {
            await _contentManager.PublishAsync(contentItem);

            var typeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);

            await _notifier.SuccessAsync(String.IsNullOrWhiteSpace(typeDefinition.DisplayName)
                ? H["Your content has been published."]
                : H["Your {0} has been published.", typeDefinition.DisplayName]);
        });
    }

    private async Task<IShape> GetProfileShapeAync(ContentItem profileContentItem, ContentTypeDefinition contentTypeDefinition, ContentItem contentItem, IShape body)
    {
        HttpContext.Features.Set(new ContentProfileFeature()
        {
            ProfileContentItem = profileContentItem,
        });

        var shape = await _shapeFactory.CreateAsync<ProfileViewModel>("Profile", async viewModel =>
        {
            viewModel.ContentType = contentTypeDefinition;
            viewModel.ProfileContentItem = profileContentItem;
            viewModel.ContentItem = contentItem;
            viewModel.Header = await _contentItemDisplayManager.BuildDisplayAsync(profileContentItem, this, "Profile");
            viewModel.Body = body;
            viewModel.Navigation = await _shapeFactory.CreateAsync("Navigation", Arguments.From(new
            {
                // The menu should be built dynamiclly to allow adding a default menu items like "List" and "Add" should be added based on the config of each 
                MenuName = $"Profile.{profileContentItem.ContentType}",
                RouteData = GetRouteData()
            }));
        });

        // allow creating Profile__[ContentType] (i.e., Profile-[ContentType].cshtml)
        shape.Metadata.Alternates.Add($"Profile__{profileContentItem.ContentType}");

        return shape;
    }

    private async Task<IActionResult> CreatePOST(string profileId, string contentTypeId, string returnUrl, bool stayOnSamePage, Func<ContentItem, Task> conditionallyPublish)
    {
        if (String.IsNullOrWhiteSpace(profileId) || String.IsNullOrWhiteSpace(contentTypeId))
        {
            return NotFound();
        }

        var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(contentTypeId);

        if (contentTypeDefinition == null)
        {
            // given an invalid contentTypeId
            return NotFound();
        }

        var profileContentItem = await _contentManager.GetAsync(profileId);

        if (profileContentItem == null)
        {
            // given an invalid profileId
            return NotFound();
        }

        var profileTypeDefinition = _contentDefinitionManager.GetTypeDefinition(profileContentItem.ContentType);

        if (profileTypeDefinition == null)
        {
            // the given profileId does not have content definition
            return NotFound();
        }

        var profileSettings = profileTypeDefinition.GetSettings<ContentProfileSettings>();

        if (profileSettings == null || profileSettings.ContainedContentTypes == null || !profileSettings.ContainedContentTypes.Contains(contentTypeDefinition.Name, StringComparer.OrdinalIgnoreCase))
        {
            // the given contentTypeId does not belong to the profile
            return NotFound();
        }

        if (!await IsAuthorizedAsync(CommonPermissions.ViewContent, profileContentItem))
        {
            // no permission to view profile
            return Forbid();
        }

        var contentItem = await CreateContentItemForOwnedByCurrentAsync(contentTypeId, profileId);

        if (!await IsAuthorizedAsync(CommonPermissions.EditContent, contentItem))
        {
            return Forbid();
        }

        var model = await _contentItemDisplayManager.UpdateEditorAsync(contentItem, _updateModelAccessor.ModelUpdater, true);

        if (ModelState.IsValid)
        {
            await _contentManager.CreateAsync(contentItem, VersionOptions.Draft);
        }

        if (!ModelState.IsValid)
        {
            await _session.CancelAsync();
            return View(model);
        }

        await conditionallyPublish(contentItem);

        /*
        if (!String.IsNullOrEmpty(returnUrl) && !stayOnSamePage)
        {
            return this.LocalRedirect(returnUrl, true);
        }

        var adminRouteValues = (await _contentManager.PopulateAspectAsync<ContentItemMetadata>(contentItem)).AdminRouteValues;

        if (!String.IsNullOrEmpty(returnUrl))
        {
            adminRouteValues.Add("returnUrl", returnUrl);
        }

        return RedirectToRoute(adminRouteValues);
        */

        if (returnUrl == null)
        {
            return RedirectToAction(nameof(Edit), new RouteValueDictionary { { nameof(profileId), profileId }, { "ContentItemId", contentItem.ContentItemId } });
        }

        if (stayOnSamePage)
        {
            return RedirectToAction(nameof(Edit), new RouteValueDictionary { { nameof(profileId), profileId }, { "ContentItemId", contentItem.ContentItemId }, { "returnUrl", returnUrl } });
        }

        return this.LocalRedirect(returnUrl, true);
    }

    private async Task<IActionResult> EditPOST(string profileId, string contentItemId, string returnUrl, bool stayOnSamePage, Func<ContentItem, Task> conditionallyPublish)
    {
        if (String.IsNullOrWhiteSpace(profileId) || String.IsNullOrWhiteSpace(contentItemId))
        {
            return NotFound();
        }

        var contentItem = await _contentManager.GetAsync(contentItemId, VersionOptions.DraftRequired);

        if (contentItem == null)
        {
            return NotFound();
        }

        if (!await IsAuthorizedAsync(CommonPermissions.EditContent, contentItem))
        {
            return Forbid();
        }

        var profilePart = contentItem.As<ContainedProfilePart>();

        if (String.IsNullOrEmpty(profilePart?.ProfileContentItemId))
        {
            return NotFound();
        }

        if (String.IsNullOrEmpty(profilePart?.ProfileContentItemId))
        {
            // At this point, this content item was not created using profile.
            // Or, it was created prior the content-type was configured as profile.
            // Redirect to standard content item 
            return RedirectToAction("Edit", "Admin", new { contentItemId });
        }

        if (profilePart.ProfileContentItemId != profileId)
        {
            // if the contained ProfileId does not equal to profile, the given contentItemId does not belong to the given profileId.
            return NotFound();
        }

        var profileContentItem = await _contentManager.GetAsync(profilePart.ProfileContentItemId);

        if (profileContentItem == null)
        {
            return NotFound();
        }

        if (!await IsAuthorizedAsync(CommonPermissions.ViewContent, profileContentItem))
        {
            return Forbid();
        }

        var model = await _contentItemDisplayManager.UpdateEditorAsync(contentItem, _updateModelAccessor.ModelUpdater, false);

        if (!ModelState.IsValid)
        {
            await _session.CancelAsync();
            return View(nameof(Edit), model);
        }

        await conditionallyPublish(contentItem);

        if (returnUrl == null)
        {
            return RedirectToAction(nameof(Edit), new RouteValueDictionary { { nameof(profileId), profileId }, { "ContentItemId", contentItem.ContentItemId } });
        }

        if (stayOnSamePage)
        {
            return RedirectToAction(nameof(Edit), new RouteValueDictionary { { nameof(profileId), profileId }, { "ContentItemId", contentItem.ContentItemId }, { "returnUrl", returnUrl } });
        }

        return this.LocalRedirect(returnUrl, true);
    }

    private async Task<ContentItem> CreateContentItemForOwnedByCurrentAsync(string contentType, string profileId)
    {
        var contentItem = await _contentManager.NewAsync(contentType);
        contentItem.Owner = CurrentUserId();
        contentItem.Weld<ContainedProfilePart>();
        contentItem.Alter<ContainedProfilePart>(part =>
        {
            part.ProfileContentItemId = profileId;
        });

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

    private RouteData GetRouteData()
    {
        var routeValues = new RouteValueDictionary(HttpContext.Request.RouteValues);

        var query = HttpContext.Request.Query;

        foreach (var key in query.Keys)
        {
            routeValues.TryAdd(key, query[key]);
        }

        return new RouteData(routeValues);
    }
}
