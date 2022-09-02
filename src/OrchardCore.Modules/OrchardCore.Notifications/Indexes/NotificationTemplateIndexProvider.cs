using OrchardCore.ContentManagement;
using OrchardCore.Notifications.Models;
using YesSql.Indexes;

namespace OrchardCore.Notifications.Indexes;

public class NotificationTemplateIndexProvider : IndexProvider<ContentItem>
{
    public override void Describe(DescribeContext<ContentItem> context)
    {
        context
            .For<NotificationTemplateIndex>()
            .Map(contentItem =>
            {
                if (contentItem == null || !contentItem.Published || contentItem.ContentType !=
                NotificationTemplateConstants.NotificationTemplateType)
                {
                    return null;
                }

                var notificationTemplate = contentItem.As<NotificationTemplatePart>();

                if (notificationTemplate == null)
                {
                    return null;
                }

                return new NotificationTemplateIndex()
                {
                    NotificationTemplateContentItemId = contentItem.ContentItemVersionId,
                    TemplateName = notificationTemplate.TemplateName
                };
            });
    }
}
