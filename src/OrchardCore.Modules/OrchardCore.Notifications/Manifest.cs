using OrchardCore.Modules.Manifest;

[assembly: Module(
    Id = "OrchardCore.Notifications",
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
        "OrchardCore.Email"
    }
)]
