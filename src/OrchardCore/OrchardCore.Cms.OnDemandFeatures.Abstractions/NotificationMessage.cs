using System.Globalization;

namespace OrchardCore.Cms.OnDemandFeatures.Abstractions;

public class NotificationMessage
{
    public string? Subject { get; set; }

    public string? TextBody { get; set; }

    public string? HtmlBody { get; set; }

    public CultureInfo? Culture { get; set; }
}
