namespace OrchardCore.Notifications;

public interface INotificationTemplateProvider
{
    string Id { get; }

    string Title { get; }

    string Description { get; }

    IEnumerable<string> GetArguments();
}
