using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "OrchardCore.Cms.OnDemandFeatures",
    Author = "The Orchard Core Team",
    Website = "https://orchardcore.net",
    Version = "0.0.1",
    Description = "OrchardCore.Cms.OnDemandFeatures",
    Category = "Content Management"
)]

[assembly: Feature(
    Id = "OC.Notification",
    EnabledByDependencyOnly = true,
    Dependencies = new[]
    {
        "OrchardCore.ContentFields"
    }
)]

[assembly: Feature(
    Id = "OC.Notification.Email",
    Dependencies = new[]
    {
        "OC.Notification",
        "OrchardCore.Email"
    }
)]

[assembly: Feature(
    Id = "OC.Notification.Push",
    Description = "Provide a way to send push notification using OneSignal implemenation.",
    Dependencies = new[]
    {
        "OC.Notification"
    }
)]
