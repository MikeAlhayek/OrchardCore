using OrchardCore.Users;

namespace OrchardCore.Cms.OnDemandFeatures.Abstractions;

public interface INotificationCoordinator
{
    Task<int> TrySendAsync(IUser user, NotificationMessage message);
}
