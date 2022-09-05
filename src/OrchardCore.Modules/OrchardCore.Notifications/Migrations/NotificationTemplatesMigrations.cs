using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using OrchardCore.Markdown.Fields;
using OrchardCore.Markdown.Settings;
using OrchardCore.Title.Models;
using YesSql.Sql;

namespace OrchardCore.Notifications.Migrations;

public class NotificationTemplatesMigrations : DataMigration
{
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public NotificationTemplatesMigrations(IContentDefinitionManager contentDefinitionManager)
    {
        _contentDefinitionManager = contentDefinitionManager;
    }

    public int Create()
    {
        _contentDefinitionManager.AlterPartDefinition(NotificationTemplateConstants.NotificationMessageTemplatePart, part => part
            .WithField("Subject", field => field
                .OfType(nameof(TextField))
                .WithDisplayName("Notification Subject")
                .WithPosition("1")
                .WithSettings(new TextFieldSettings()
                {
                    Required = true,
                })
            )
            .WithField("Body", field => field
                .OfType(nameof(MarkdownField))
                .WithDisplayName("Notification Body")
                .WithPosition("2")
                .WithEditor("TextArea")
                .WithSettings(new MarkdownFieldSettings()
                {
                    SanitizeHtml = true,
                })
            )
        );

        _contentDefinitionManager.AlterPartDefinition(NotificationTemplateConstants.NotificationReceivingUsersPart, part => part
           .Attachable()
           .WithDisplayName("Notification Receiving Users")
           .WithDescription("Provides options for who to send an email notifcation too.")
           .WithField("Users", part => part
                .OfType(nameof(UserPickerField))
                .WithDisplayName("Users who will recieve the notification.")
                .WithPosition("1")
                .MergeSettings<UserPickerFieldSettings>(settings =>
                {
                    settings.Required = false;
                    settings.DisplayAllUsers = true;
                    settings.Multiple = true;
                    settings.Hint = "Please select the user(s) who will recieve the notification.";
                })
            )
        );

        _contentDefinitionManager.AlterTypeDefinition(NotificationTemplateConstants.NotificationTemplateType, type => type
           .Creatable()
           .Listable()
           .DisplayedAs("Notification Template")
           .WithPart(nameof(TitlePart), part => part
               .WithSettings(new TitlePartSettings()
               {
                   Options = TitlePartOptions.EditableRequired,
               })
           )
           .WithPart(NotificationTemplateConstants.NotificationTemplatePart, part => part.WithPosition("1"))
           .WithPart(NotificationTemplateConstants.NotificationMessageTemplatePart, part => part.WithPosition("2"))
           .WithPart(NotificationTemplateConstants.NotificationReceiverPart, part => part.WithPosition("3"))
           .WithPart(NotificationTemplateConstants.NotificationReceivingUsersPart, part => part.WithPosition("4"))
       );

        SchemaBuilder.CreateMapIndexTable<NotificationTemplateIndex>(table => table
            .Column<string>("NotificationTemplateContentItemId", column => column.WithLength(26))
            .Column<string>("TemplateName", column => column.WithLength(255))
        );

        return 1;
    }
}
