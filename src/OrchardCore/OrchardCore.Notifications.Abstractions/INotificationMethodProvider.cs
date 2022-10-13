using System.Threading.Tasks;
using OrchardCore.Users;

namespace OrchardCore.Notifications;

public interface INotificationMethodProvider
{
    /// <summary>
    /// Unique name for the provider
    /// </summary>
    string Method { get; }

    /// <summary>
    /// A localized name for the method.
    /// </summary>
    string Name { get; }

    Task<bool> TrySendAsync(IUser user, NotificationMessage message);
}
