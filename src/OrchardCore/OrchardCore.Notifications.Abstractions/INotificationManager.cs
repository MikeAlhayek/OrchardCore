using OrchardCore.Users;

namespace OrchardCore.Notifications;

public interface INotificationManager
{
    Task<int> TrySendAsync(IUser user, NotificationMessage message);
}
