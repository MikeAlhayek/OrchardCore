using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.Cms.OnDemandFeatures.Abstractions;
using OrchardCore.Cms.OnDemandFeatures.Controllers;
using OrchardCore.Cms.OnDemandFeatures.Core;
using OrchardCore.Cms.OnDemandFeatures.Drivers;
using OrchardCore.Cms.OnDemandFeatures.Migrations;
using OrchardCore.Cms.OnDemandFeatures.Models;
using OrchardCore.Cms.OnDemandFeatures.Services;
using OrchardCore.ContentManagement;
using OrchardCore.Data.Migration;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Modules;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;
using OrchardCore.Users.Models;

namespace OrchardCore.Cms.OnDemandFeatures;


[Feature("OC.Notification")]
public class Startup : StartupBase
{
    private readonly AdminOptions _adminOption;
    private readonly IShellConfiguration _configuration;

    public Startup(IOptions<AdminOptions> adminOption, IShellConfiguration configuration)
    {
        _adminOption = adminOption.Value;
        _configuration = configuration;
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IDisplayDriver<User>, UserNotificationPartDisplayDriver>();
        services.AddScoped<INotificationCoordinator, NotificationCoordinator>();
        services.AddScoped<IDataMigration, NotifyUserMigrations>();
        services.AddContentPart<NotifyUserPart>();
        services.AddScoped<INavigationProvider, AdminMenu>();
        services.AddScoped<IPermissionProvider, NotifyPermissionProvider>();
        services.Configure<NotificationCoordinatorOptions>(_configuration.GetSection("OrchardCore_Notification:Coordinator"));
    }

    public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        routes.MapAreaControllerRoute(
            name: "OnDemandFeaturesAdmin",
            areaName: "OrchardCore.Cms.OnDemandFeatures",
            pattern: _adminOption.AdminUrlPrefix + "/OnDemandFeatures",
            defaults: new { controller = typeof(AdminController).ControllerName(), action = nameof(AdminController.Notify) }
        );
    }
}

[Feature("OC.Notification.Email")]
public class EmailStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<INotificationSender, EmailNotificationSender>();
    }
}


[Feature("OC.Notification.Push")]
public class PushStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<INotificationSender, PushNotificationSender>();
        services.AddScoped<IPushNotificationService, OneSignalPushNotificationService>();
    }
}

[Feature("OC.Notification.Web")]
public class WebStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<INotificationSender, WebNotificationSender>();
    }
}
