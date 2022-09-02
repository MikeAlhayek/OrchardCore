namespace OrchardCore.Notifications;

public interface INotificationTemplateProvider
{
    string Name { get; }

    string Description { get; }

    IEnumerable<string> GetArguments();
}
