using System.Collections.Generic;

namespace OrchardCore.Notifications.Handlers;

public class ContentsNotificationTemplateProvider : INotificationTemplateProvider
{
    public const string Key = "contents";

    public string Id => Key;

    public string Title => "Content based notification";

    public string Description => "Send notifications based on content event using content type settings.";

    public IEnumerable<string> GetArguments() => new List<string>()
    {
        "DisplayText",
        "ContentItemId",
        "ContentType",
        "ModifiedUtc",
        "PublishedUtc",
        "CreatedUtc",
        "Owner",
        "Author",
    };
}
