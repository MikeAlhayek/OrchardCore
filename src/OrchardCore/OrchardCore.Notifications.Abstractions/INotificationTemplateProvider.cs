namespace OrchardCore.Notifications;

public interface INotificationTemplateProvider
{
    string Id { get; }

    string Title { get; }

    string Description { get; }

    NotificationTemplateMetadata Metadata { get; }

    IEnumerable<string> GetArguments();
}
