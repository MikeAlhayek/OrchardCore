using OrchardCore.Users;

namespace OrchardCore.Notifications;

public class NotificationMessageContext : NotificationMessage
{
    public List<IUser> Users { get; }

    public NotificationMessageContext()
    {
        Users = new List<IUser>();
    }
}
