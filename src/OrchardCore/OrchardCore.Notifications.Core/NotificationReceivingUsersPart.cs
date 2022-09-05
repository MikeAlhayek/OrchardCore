using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;

namespace OrchardCore.Notifications.Models;

public class NotificationReceivingUsersPart : ContentPart
{
    public UserPickerField Users { get; set; }
}
