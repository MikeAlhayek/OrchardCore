using OrchardCore.ContentManagement;

namespace OrchardCore.Notifications;

public interface INotificationMessageProvider
{
    Task<IEnumerable<NotificationMessageContext>> GetAsync(string template, Dictionary<string, string> messageArguments);
    Task<IEnumerable<NotificationMessageContext>> GetAsync(IEnumerable<ContentItem> templates, Dictionary<string, string> messageArguments);
}
