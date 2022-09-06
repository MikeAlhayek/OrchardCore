using OrchardCore.Modules.Manifest;

[assembly: Module(
    Author = "The Orchard Core Team",
    Website = "https://orchardcore.net",
    Version = "1.0.0"
)]

[assembly: Feature(
    Id = "OrchardCore.Notifications",
    Name = "Notifications",
    Description = "Provides a notification infrastructure.",
    Category = "Notifications",
    EnabledByDependencyOnly = true
)]

[assembly: Feature(
    Id = "OrchardCore.Notifications.Emails",
    Name = "Email Notifications",
    Description = "Provides a way to send email notifications for users",
    Category = "Notifications",
    Dependencies = new[]
    {
        "OrchardCore.Notifications",
        "OrchardCore.Contents",
        "OrchardCore.Email"
    }
)]

[assembly: Feature(
    Id = "OrchardCore.Notifications.Templates",
    Name = "Notification Templates",
    Description = "Provides a way to create notification based templates.",
    Dependencies = new[] {
        "OrchardCore.ContentFields",
        "OrchardCore.Notifications",
        "OrchardCore.Markdown",
        "OrchardCore.Title"
    },
    Category = "Notifications"
)]

[assembly: Feature(
    Id = "OrchardCore.Notifications.ContentTemplates",
    Name = "Notification Templates for Contents",
    Description = "Provides a way to create notification using content events.",
    Dependencies = new[] {
        "OrchardCore.ContentTypes",
        "OrchardCore.Notifications.Templates"
    },
    Category = "Notifications"
)]
