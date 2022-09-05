using System.Collections.Generic;

namespace OrchardCore.Notifications.Handlers;

public class NewUserCreatedNotificationTemplateProvider : INotificationTemplateProvider
{
    public const string Key = "new-user-created";

    public string Id => Key;

    public string Title => "New user notification";

    public string Description => "Send welcome notifications when user is created.";

    public NotificationTemplateMetadata Metadata => new();

    public IEnumerable<string> GetArguments() => new List<string>()
    {
        "username",
        "password",
        "email"
    };
}
