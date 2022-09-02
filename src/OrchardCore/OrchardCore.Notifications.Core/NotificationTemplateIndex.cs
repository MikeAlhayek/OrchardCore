using YesSql.Indexes;

namespace OrchardCore.Notifications;

public class NotificationTemplateIndex : MapIndex
{
    public string NotificationTemplateContentItemId { get; set; }

    public string TemplateName { get; set; }
}
