using OrchardCore.Users;

namespace OrchardCore.Notifications;

public interface INotificationTemplateDispatcher
{
    Task SendAsync(string template, IUser user, Dictionary<string, string> arguments, object resource);
}
