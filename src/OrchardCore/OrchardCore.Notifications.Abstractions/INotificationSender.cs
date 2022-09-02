using OrchardCore.Users;

namespace OrchardCore.Notifications.Abstractions;

public interface INotificationSender
{
    // Unique name for the sender service
    string Type { get; }

    // the localized name for the server service. this value will be visible to the user
    string Name { get; }

    Task<bool> TrySendAsync(IUser user, NotificationMessage message);
}
