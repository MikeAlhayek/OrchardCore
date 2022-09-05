using OrchardCore.ContentManagement;

namespace OrchardCore.Notifications.Models;

public class NotificationReceiverPart : ContentPart
{
    public string[] Receivers { get; set; }
}
