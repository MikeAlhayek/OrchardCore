using OrchardCore.ContentManagement;

namespace OrchardCore.Notifications;

public class UserNotificationPart : ContentPart
{
    public string[] Types { get; set; }
}
