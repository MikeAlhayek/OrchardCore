using System;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Contents.Models;
using OrchardCore.Contents.ViewModels;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Contents.Drivers;

public class ContentProfileSettingsDisplayDriver : ContentTypeDefinitionDisplayDriver
{
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public ContentProfileSettingsDisplayDriver(IContentDefinitionManager contentDefinitionManager)
    {
        _contentDefinitionManager = contentDefinitionManager;
    }

    public override IDisplayResult Edit(ContentTypeDefinition contentTypeDefinition)
    {
        return Initialize<ContentProfileSettingsViewModel>("ContentProfileSettings_Edit", model =>
        {
            var settings = contentTypeDefinition.GetSettings<ContentProfileSettings>();

            model.DisplayMode = settings.DisplayMode;
            var containedTypes = settings.ContainedContentTypes ?? Array.Empty<string>();
            model.ContainedContentTypes = containedTypes;
            model.DisplayAsProfile = containedTypes.Length > 0;
        }).Location("Content:5.5");
    }

    public override async Task<IDisplayResult> UpdateAsync(ContentTypeDefinition contentTypeDefinition, UpdateTypeEditorContext context)
    {
        var model = new ContentProfileSettingsViewModel();

        if (await context.Updater.TryUpdateModelAsync(model, Prefix))
        {
            var selected = Array.Empty<string>();

            if (model.DisplayAsProfile && model.ContainedContentTypes != null)
            {
                selected = _contentDefinitionManager.ListTypeDefinitions()
                .Where(contentType => contentType.IsListable()
                    && !contentTypeDefinition.Name.Equals(contentType.Name, StringComparison.OrdinalIgnoreCase)
                    && model.ContainedContentTypes.Contains(contentType.Name, StringComparer.OrdinalIgnoreCase))
                .Select(contentType => contentType.Name)
                .ToArray();
            }

            context.Builder.WithSettings(new ContentProfileSettings()
            {
                DisplayMode = model.DisplayMode,
                ContainedContentTypes = selected,
            });
        }

        return Edit(contentTypeDefinition);
    }
}
