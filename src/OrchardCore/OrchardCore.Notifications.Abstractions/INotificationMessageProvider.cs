namespace OrchardCore.Notifications;

public interface INotificationMessageProvider
{
    Task<IEnumerable<NotificationMessageContext>> GetAsync(string template, Dictionary<string, string> arguments);
}
