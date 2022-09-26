using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Contents.Models;
using OrchardCore.Contents.Services;
using OrchardCore.Contents.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Mvc.ModelBinding;

namespace OrchardCore.Contents.Drivers;

public class ProfileContentDriver : ContentDisplayDriver
{
    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly IContentManager _contentManager;
    private readonly ProfileContentTypeProvider _profileContentTypeProvider;
    private readonly IStringLocalizer<ProfileContentDriver> S;

    public ProfileContentDriver(IContentDefinitionManager contentDefinitionManager,
        IContentManager contentManager,
        ProfileContentTypeProvider profileContentTypeProvider,
        IStringLocalizer<ProfileContentDriver> stringLocalizer)
    {
        _contentDefinitionManager = contentDefinitionManager;
        _contentManager = contentManager;
        _profileContentTypeProvider = profileContentTypeProvider;
        S = stringLocalizer;
    }

    public override bool CanHandleModel(ContentItem model)
    {
        if (String.IsNullOrEmpty(model.ContentType))
        {
            return false;
        }

        var profilePart = model.As<ContainedProfilePart>();

        if (!String.IsNullOrEmpty(profilePart?.ProfileContentItemId))
        {
            return false;
        }

        return _profileContentTypeProvider.GetProfileType(model.ContentType) != null;
    }

    public override Task<IDisplayResult> EditAsync(ContentItem model, BuildEditorContext context)
    {
        return Task.FromResult<IDisplayResult>(Initialize<ProfilePickerViewModel>("ProfilePicker_Edit", async viewModel =>
        {
            var part = model.As<SelectedProfilePart>();

            viewModel.ContentItemId = part?.ProfileContentItemId;
            viewModel.SelectedItems = new List<VueMultiselectItemViewModel>();
            var profileType = _profileContentTypeProvider.GetProfileType(model.ContentType);
            viewModel.ContentType = profileType.Name;
            if (String.IsNullOrEmpty(viewModel.ContentItemId))
            {
                return;
            }

            var contentItem = await _contentManager.GetAsync(viewModel.ContentItemId, VersionOptions.Latest);

            if (contentItem != null)
            {
                viewModel.SelectedItems.Add(new VueMultiselectItemViewModel
                {
                    Id = contentItem.ContentItemId,
                    DisplayText = contentItem.ToString(),
                    HasPublished = await _contentManager.HasPublishedVersionAsync(contentItem)
                });
            }
        }).Location("Parts"));
    }

    public override async Task<IDisplayResult> UpdateAsync(ContentItem model, UpdateEditorContext context)
    {
        var viewModel = new ProfilePickerViewModel();

        if (await context.Updater.TryUpdateModelAsync(viewModel, Prefix))
        {
            var contentItem = await _contentManager.GetAsync(viewModel.ContentItemId, VersionOptions.Published);

            if (contentItem == null)
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(ProfilePickerViewModel.ContentItemId), S["Invalid profile selected."]);
            }
            else
            {
                var contentType = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);

                var profileSettings = contentType.GetSettings<ContentProfileSettings>();

                if (profileSettings.ContainedContentTypes == null
                    || !profileSettings.ContainedContentTypes.Contains(model.ContentType, StringComparer.OrdinalIgnoreCase))
                {
                    context.Updater.ModelState.AddModelError(Prefix, nameof(ProfilePickerViewModel.ContentItemId), S["Invalid profile selected."]);
                }

                if (context.Updater.ModelState.IsValid)
                {
                    model.Alter<ContainedProfilePart>(part =>
                    {
                        part.ProfileContentItemId = viewModel.ContentItemId;
                    });

                    if (model.Has<SelectedProfilePart>())
                    {
                        model.Remove<SelectedProfilePart>();
                    }
                }
                else
                {
                    model.Alter<SelectedProfilePart>(part =>
                    {
                        part.ProfileContentItemId = viewModel.ContentItemId;
                    });
                }
            }
        }

        return await EditAsync(model, context);
    }
}
