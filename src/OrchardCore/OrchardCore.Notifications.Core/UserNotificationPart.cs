using OrchardCore.ContentManagement;

namespace OrchardCore.Notifications.Core;

public class UserNotificationPart : ContentPart
{
    public string[] Types { get; set; }
}
