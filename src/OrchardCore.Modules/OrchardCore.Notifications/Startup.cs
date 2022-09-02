using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.Data.Migration;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Modules;
using OrchardCore.Notifications.Drivers;
using OrchardCore.Notifications.Handlers;
using OrchardCore.Notifications.Indexes;
using OrchardCore.Notifications.Migrations;
using OrchardCore.Notifications.Models;
using OrchardCore.Users.Handlers;
using OrchardCore.Users.Models;
using YesSql.Indexes;

namespace OrchardCore.Notifications;

[Feature("OrchardCore.Notifications")]
public class Startup : StartupBase
{
    private readonly IShellConfiguration _configuration;

    public Startup(IShellConfiguration configuration)
    {
        _configuration = configuration;
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IDisplayDriver<User>, UserNotificationPartDisplayDriver>();
        services.AddScoped<INotificationManager, NotificationManager>();
        services.Configure<NotificationOptions>(_configuration.GetSection("OrchardCore_Notifications"));
    }
}

[Feature("OrchardCore.Notifications.Email")]
public class EmailNotificationsStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<INotificationSender, EmailNotificationSender>();
    }
}

[Feature("OrchardCore.Notifications.Templates")]
public class NotificationTemplatesStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddContentPart<NotificationMessageTemplatePart>();
        services.AddContentPart<NotificationTemplateDeliveryPart>();

        services.AddContentPart<NotificationTemplatePart>()
            .UseDisplayDriver<NotificationTemplatePartDisplayDriver>();

        services.AddScoped<IDataMigration, NotificationTemplatesMigrations>();
        services.AddSingleton<IIndexProvider, NotificationTemplateIndexProvider>();
        services.AddScoped<INotificationTemplateDispatcher, DefaultTemplateDispatcher>();
        services.AddScoped<INotificationMessageProvider, NotificationMessageProvider>();
    }
}

[RequireFeatures("OrchardCore.Notifications.Templates", "OrchardCore.Email")]
public class NewUserNotificationTemplatesStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<INotificationTemplateProvider, NewUserCreatedNotificationTemplateProvider>();
        services.AddScoped<IUserEventHandler, DispatchTemplateWhenUserCreated>();
    }
}
