using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.Markdown.Fields;

namespace OrchardCore.Notifications.Models;

public class NotificationMessageTemplatePart : ContentPart
{
    public TextField Subject { get; set; }

    public MarkdownField Body { get; set; }
}
