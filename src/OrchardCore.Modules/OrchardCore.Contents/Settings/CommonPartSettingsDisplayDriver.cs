using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Contents.Models;
using OrchardCore.Contents.ViewModels;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Lists.Settings
{
    public class CommonPartSettingsDisplayDriver : ContentTypePartDefinitionDisplayDriver<CommonPart>
    {
        public override IDisplayResult Edit(ContentTypePartDefinition contentTypePartDefinition, IUpdateModel updater)
        {
            return Initialize<CommonPartSettingsViewModel>("CommonPartSettings_Edit", model =>
            {
                var settings = contentTypePartDefinition.GetSettings<CommonPartSettings>();
                model.DisplayDateEditor = settings.DisplayDateEditor;
                model.DisplayOwnerEditor = settings.DisplayOwnerEditor;
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentTypePartDefinition contentTypePartDefinition, UpdateTypePartEditorContext context)
        {
            var model = new CommonPartSettingsViewModel();

            if (await context.Updater.TryUpdateModelAsync(model, Prefix))
            {
                // CommonPartSettings could be set by another driver. Get existing settings first, then update it.
                var settings = contentTypePartDefinition.GetSettings<CommonPartSettings>();
                settings.DisplayOwnerEditor = model.DisplayOwnerEditor;
                settings.DisplayDateEditor = model.DisplayDateEditor;

                context.Builder.WithSettings(settings);
            }

            return Edit(contentTypePartDefinition, context.Updater);
        }
    }
}
