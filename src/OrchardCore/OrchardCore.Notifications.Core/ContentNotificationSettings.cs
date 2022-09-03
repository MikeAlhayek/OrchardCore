using System;

namespace OrchardCore.Notifications;

public class ContentNotificationSettings
{
    public bool SendNotification { get; set; }

    public string EventName { get; set; }

    public string[] NotificationTemplateContentItemIds { get; set; } = Array.Empty<string>();
}
