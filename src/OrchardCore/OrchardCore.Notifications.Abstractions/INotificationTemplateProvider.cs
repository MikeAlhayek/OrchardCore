namespace OrchardCore.Notifications;

public interface INotificationTemplateProvider
{
    public string Name { get; }
    public string Description { get; }
    public IEnumerable<string> GetArguments();
}
