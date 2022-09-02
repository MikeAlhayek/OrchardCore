using OrchardCore.Users;

namespace OrchardCore.Notifications.Abstractions;

public interface INotificationManager
{
    Task<int> TrySendAsync(IUser user, NotificationMessage message);
}
