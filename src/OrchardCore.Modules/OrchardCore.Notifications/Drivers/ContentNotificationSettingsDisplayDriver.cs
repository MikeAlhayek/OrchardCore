using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Records;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Notifications.ViewModels;
using YesSql;

namespace OrchardCore.Notifications.Drivers;

public class ContentNotificationSettingsDisplayDriver : ContentTypeDefinitionDisplayDriver
{
    private readonly IStringLocalizer S;
    private readonly ISession _session;

    public ContentNotificationSettingsDisplayDriver(IStringLocalizer<ContentNotificationSettingsDisplayDriver> stringLocalizer
        , ISession session)
    {
        S = stringLocalizer;
        _session = session;
    }

    public override IDisplayResult Edit(ContentTypeDefinition contentTypeDefinition)
    {
        return Initialize<ContentNotificationSettingsViewModel>("ContentNotificationSettings_Edit", async model =>
        {
            var settings = contentTypeDefinition.GetSettings<ContentNotificationSettings>();

            model.SendNotification = settings.SendNotification;
            model.EventName = settings.EventName;
            model.NotificationTemplateContentItemIds = settings.NotificationTemplateContentItemIds ?? Array.Empty<string>();
            model.EventNames = GetEventNames().OrderBy(x => x.Text);
            model.NotificationTemplates = (await _session
            .QueryIndex<ContentItemIndex>(index => index.ContentType == NotificationTemplateConstants.NotificationTemplateType && index.Published)
            .OrderBy(index => index.DisplayText)
            .ListAsync())
            .Select(x => new SelectListItem(x.DisplayText, x.ContentItemId));
        }).Location("Content:7");
    }

    public override async Task<IDisplayResult> UpdateAsync(ContentTypeDefinition contentTypeDefinition, UpdateTypeEditorContext context)
    {
        var model = new ContentNotificationSettingsViewModel();

        if (await context.Updater.TryUpdateModelAsync(model, Prefix))
        {
            if (model.SendNotification)
            {
                var eventName = GetEventNames().FirstOrDefault(x => String.Equals(x.Value, model.EventName, StringComparison.OrdinalIgnoreCase));

                if (eventName == null)
                {
                    context.Updater.ModelState.AddModelError(Prefix, nameof(model.EventName), S["Event name is required."]);
                }
                if (model.NotificationTemplateContentItemIds == null || model.NotificationTemplateContentItemIds.Length == 0)
                {
                    context.Updater.ModelState.AddModelError(Prefix, nameof(model.NotificationTemplateContentItemIds), S["At least one template is required."]);
                }

                context.Builder.WithSettings(new ContentNotificationSettings
                {
                    SendNotification = true,
                    EventName = model.EventName,
                    NotificationTemplateContentItemIds = model.NotificationTemplateContentItemIds
                });

            }
            else
            {
                context.Builder.WithSettings(new ContentNotificationSettings());
            }
        }

        return Edit(contentTypeDefinition);
    }

    private static IEnumerable<SelectListItem> GetEventNames()
    {
        yield return new SelectListItem("Activating", "Activating");
        yield return new SelectListItem("Activated", "Activated");
        yield return new SelectListItem("Initializing", "Initializing");
        yield return new SelectListItem("Initialized", "Initialized");
        yield return new SelectListItem("Creating", "Creating");
        yield return new SelectListItem("Created", "Created");
        yield return new SelectListItem("Loading", "Loading");
        yield return new SelectListItem("Loaded", "Loaded");
        yield return new SelectListItem("Importing", "Importing");
        yield return new SelectListItem("Imported", "Imported");
        yield return new SelectListItem("Updating", "Updating");
        yield return new SelectListItem("Updated", "Updated");
        yield return new SelectListItem("Validating", "Validating");
        yield return new SelectListItem("Validated", "Validated");
        yield return new SelectListItem("Restoring", "Restoring");
        yield return new SelectListItem("Restored", "Restored");
        yield return new SelectListItem("Versioning", "Versioning");
        yield return new SelectListItem("Versioned", "Versioned");
        yield return new SelectListItem("DraftSaving", "DraftSaving");
        yield return new SelectListItem("Publishing", "Publishing");
        yield return new SelectListItem("Published", "Published");
        yield return new SelectListItem("Unpublishing", "Unpublishing");
        yield return new SelectListItem("Unpublished", "Unpublished");
        yield return new SelectListItem("Removing", "Removing");
        yield return new SelectListItem("Removed", "Removed");
        yield return new SelectListItem("Cloning", "Cloning");
        yield return new SelectListItem("Cloned", "Cloned");
    }
}
