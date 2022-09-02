using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using OrchardCore.Markdown.Fields;
using OrchardCore.Markdown.Settings;
using OrchardCore.Notifications.Models;
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
                .WithDisplayName("Email's Subject")
                .WithPosition("1")
                .WithSettings(new TextFieldSettings()
                {
                    Required = true,
                })
            )
            .WithField("Body", field => field
                .OfType(nameof(MarkdownField))
                .WithDisplayName("Email's Body")
                .WithPosition("2")
                .WithEditor("TextArea")
                .WithSettings(new MarkdownFieldSettings()
                {
                    SanitizeHtml = true,
                })
            )
        );

        _contentDefinitionManager.AlterPartDefinition(NotificationTemplateConstants.NotificationTemplateDeliveryPart, part => part
           .Attachable()
           .WithDisplayName("Notification Template Delivery Part")
           .WithDescription("Provides options for who to send an email notifcation too.")
           .WithField("SendTo", field => field
               .OfType(nameof(TextField))
               .WithDisplayName("Send To")
               .WithPosition("1")
               .WithEditor("PredefinedList")
               .MergeSettings<TextFieldPredefinedListEditorSettings>(settings =>
               {
                   settings.Editor = EditorOption.Dropdown;
                   settings.DefaultValue = NotificationTemplateConstants.CurrentUserValue;
                   settings.Options = new[]
                   {
                       new ListValueOption()
                       {
                           Name = "Current User",
                           Value = NotificationTemplateConstants.CurrentUserValue,
                       },
                       new ListValueOption()
                       {
                           Name = "Specific User(s)",
                           Value = NotificationTemplateConstants.SpecificUsersValue,
                       },
                   };
               })
            )
           .WithField(nameof(NotificationTemplateDeliveryPart.Users), part => part
                .OfType(nameof(UserPickerField))
                .WithDisplayName("Users who can recieve an email notification.")
                .WithPosition("2")
                .MergeSettings<UserPickerFieldSettings>(settings =>
                {
                    settings.Required = false;
                    settings.DisplayAllUsers = true;
                    settings.Multiple = true;
                    settings.Hint = "Please select the user(s) who will recieve an email notification.";
                })
            )
        );

        _contentDefinitionManager.AlterTypeDefinition(NotificationTemplateConstants.NotificationTemplateType, type => type
           .DisplayedAs("Notification Template")
           .Creatable()
           .Listable()
           .WithPart(nameof(TitlePart), part => part
               .WithSettings(new TitlePartSettings()
               {
                   Options = TitlePartOptions.EditableRequired,
               })
           )
           .WithPart(NotificationTemplateConstants.NotificationTemplatePart, field => field.WithPosition("1"))
           .WithPart(NotificationTemplateConstants.NotificationMessageTemplatePart, field => field.WithPosition("2"))
           .WithPart(NotificationTemplateConstants.NotificationTemplateDeliveryPart, field => field.WithPosition("3"))
       );

        SchemaBuilder.CreateMapIndexTable<NotificationTemplateIndex>(table => table
            .Column<string>("NotificationTemplateContentItemId", column => column.WithLength(26))
            .Column<string>("TemplateName", column => column.WithLength(255))
        );

        return 1;
    }
}
