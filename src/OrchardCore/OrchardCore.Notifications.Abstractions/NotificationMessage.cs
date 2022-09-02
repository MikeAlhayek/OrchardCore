using System.Globalization;

namespace OrchardCore.Notifications;

public class NotificationMessage
{
    public string? Subject { get; set; }

    public string? TextBody { get; set; }

    public string? HtmlBody { get; set; }

    public CultureInfo? Culture { get; set; }
}
