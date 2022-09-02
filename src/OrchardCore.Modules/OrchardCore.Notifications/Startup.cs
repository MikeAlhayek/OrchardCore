using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Modules;
using OrchardCore.Notifications.Abstractions;
using OrchardCore.Notifications.Core;
using OrchardCore.Notifications.Drivers;
using OrchardCore.Users.Models;

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
