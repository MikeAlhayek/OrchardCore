using System.Collections.Generic;

namespace OrchardCore.Notifications.Handlers;

public class NewUserCreatedNotificationTemplateProvider : INotificationTemplateProvider
{
    public const string Key = "new-user-created";

    public const string Title = "New user notification";

    public string Name => Key;

    public string Description => "Send welcome notifications when user is created.";

    public IEnumerable<string> GetArguments() => new List<string>()
    {
        "username",
        "password",
        "email"
    };
}
