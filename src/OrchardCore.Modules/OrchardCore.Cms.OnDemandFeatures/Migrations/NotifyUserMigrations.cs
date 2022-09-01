using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;

namespace OrchardCore.Cms.OnDemandFeatures.Migrations;

public class NotifyUserMigrations : DataMigration
{
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public NotifyUserMigrations(IContentDefinitionManager contentDefinitionManager)
    {
        _contentDefinitionManager = contentDefinitionManager;
    }

    public int Create()
    {
        _contentDefinitionManager.AlterPartDefinition("NotifyUserPart", part => part
            .WithField("Subject", field => field
                .WithPosition("1")
                .OfType(nameof(TextField))
                .WithDisplayName("Subject")
                .WithSettings(new TextFieldSettings()
                {
                    Required = true,
                })
            )
            .WithField("Body", field => field
                .WithPosition("2")
                .OfType(nameof(TextField))
                .WithDisplayName("Body")
                .WithEditor("TextArea")
                .WithSettings(new TextFieldSettings()
                {
                    Required = true,
                })
            )
            .WithField("Users", field => field
                .WithPosition("3")
                .OfType(nameof(UserPickerField))
                .WithDisplayName("Users")
                .WithSettings(new UserPickerFieldSettings()
                {
                    DisplayAllUsers = true,
                    Multiple = true,
                    Required = true,
                    Hint = "Users to notify. Each user will be notified based on their notification type preferences.",
                })
            )
        );

        _contentDefinitionManager.AlterTypeDefinition("UserNotifier", type => type
            .WithPart("NotifyUserPart")
        );

        return 1;
    }
}
