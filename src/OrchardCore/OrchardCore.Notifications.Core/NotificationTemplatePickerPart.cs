using OrchardCore.ContentManagement;

namespace OrchardCore.Notifications.Core;

public class NotificationTemplatePickerPart : ContentPart
{
    public string EventName { get; set; }

    public string[] NotificationTemplateContentItemIds { get; set; }
}
