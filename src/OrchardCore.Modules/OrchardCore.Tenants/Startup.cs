using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using OrchardCore.Admin;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Distributed;
using OrchardCore.Modules;
using OrchardCore.Modules.FileProviders;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;
using OrchardCore.Setup;
using OrchardCore.Tenants.Controllers;
using OrchardCore.Tenants.Services;

namespace OrchardCore.Tenants
{
    public class Startup : StartupBase
    {
        private readonly AdminOptions _adminOptions;

        public Startup(IOptions<AdminOptions> adminOptions)
        {
            _adminOptions = adminOptions.Value;
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<INavigationProvider, AdminMenu>();
            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddScoped<ITenantValidator, TenantValidator>();
            services.AddSetup();
        }

        public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            var adminControllerName = typeof(AdminController).ControllerName();

            routes.MapAreaControllerRoute(
                name: "Tenants",
                areaName: "OrchardCore.Tenants",
                pattern: _adminOptions.AdminUrlPrefix + "/Tenants",
                defaults: new { controller = adminControllerName, action = nameof(AdminController.Index) }
            );
            routes.MapAreaControllerRoute(
                name: "TenantsCreate",
                areaName: "OrchardCore.Tenants",
                pattern: _adminOptions.AdminUrlPrefix + "/Tenants/Create",
                defaults: new { controller = adminControllerName, action = nameof(AdminController.Create) }
            );
            routes.MapAreaControllerRoute(
                name: "TenantsEdit",
                areaName: "OrchardCore.Tenants",
                pattern: _adminOptions.AdminUrlPrefix + "/Tenants/Edit/{id}",
                defaults: new { controller = adminControllerName, action = nameof(AdminController.Edit) }
            );
            routes.MapAreaControllerRoute(
                name: "TenantsReload",
                areaName: "OrchardCore.Tenants",
                pattern: _adminOptions.AdminUrlPrefix + "/Tenants/Reload/{id}",
                defaults: new { controller = adminControllerName, action = nameof(AdminController.Reload) }
            );
        }
    }

    [Feature("OrchardCore.Tenants.FileProvider")]
    public class FileProviderStartup : StartupBase
    {
        /// <summary>
        /// The path in the tenant's App_Data folder containing the files
        /// </summary>
        private const string AssetsPath = "wwwroot";

        // Run after other middlewares
        public override int Order => 10;

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ITenantFileProvider>(serviceProvider =>
            {
                var shellOptions = serviceProvider.GetRequiredService<IOptions<ShellOptions>>();
                var shellSettings = serviceProvider.GetRequiredService<ShellSettings>();

                string contentRoot = GetContentRoot(shellOptions.Value, shellSettings);

                if (!Directory.Exists(contentRoot))
                {
                    Directory.CreateDirectory(contentRoot);
                }
                return new TenantFileProvider(contentRoot);
            });

            services.AddSingleton<IStaticFileProvider>(serviceProvider =>
            {
                return serviceProvider.GetRequiredService<ITenantFileProvider>();
            });
        }

        public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            var tenantFileProvider = serviceProvider.GetRequiredService<ITenantFileProvider>();

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = tenantFileProvider,
                DefaultContentType = "application/octet-stream",
                ServeUnknownFileTypes = true,

                // Cache the tenant static files for 30 days
                OnPrepareResponse = ctx =>
                {
                    ctx.Context.Response.Headers[HeaderNames.CacheControl] = $"public, max-age={TimeSpan.FromDays(30).TotalSeconds}, s-max-age={TimeSpan.FromDays(365.25).TotalSeconds}";
                }
            });
        }

        private string GetContentRoot(ShellOptions shellOptions, ShellSettings shellSettings)
        {
            return Path.Combine(shellOptions.ShellsApplicationDataPath, shellOptions.ShellsContainerName, shellSettings.Name, AssetsPath);
        }
    }

    [Feature("OrchardCore.Tenants.Distributed")]
    public class DistributedStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<DistributedShellMarkerService>();
        }
    }

    [Feature("OrchardCore.Tenants.FeatureProfiles")]
    public class FeatureProfilesStartup : StartupBase
    {
        private readonly AdminOptions _adminOptions;

        public FeatureProfilesStartup(IOptions<AdminOptions> adminOptions)
        {
            _adminOptions = adminOptions.Value;
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<INavigationProvider, FeatureProfilesAdminMenu>();
            services.AddScoped<FeatureProfilesManager>();
            services.AddScoped<IFeatureProfilesService, FeatureProfilesService>();
            services.AddScoped<IFeatureProfilesSchemaService, FeatureProfilesSchemaService>();
        }

        public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            var featureProfilesControllerName = typeof(FeatureProfilesController).ControllerName();

            routes.MapAreaControllerRoute(
                name: "TenantFeatureProfilesIndex",
                areaName: "OrchardCore.Tenants",
                pattern: _adminOptions.AdminUrlPrefix + "/TenantFeatureProfiles",
                defaults: new { controller = featureProfilesControllerName, action = nameof(FeatureProfilesController.Index) }
            );

            routes.MapAreaControllerRoute(
                name: "TenantFeatureProfilesCreate",
                areaName: "OrchardCore.Tenants",
                pattern: _adminOptions.AdminUrlPrefix + "/TenantFeatureProfiles/Create",
                defaults: new { controller = featureProfilesControllerName, action = nameof(FeatureProfilesController.Create) }
            );

            routes.MapAreaControllerRoute(
                name: "TenantFeatureProfilesEdit",
                areaName: "OrchardCore.Tenants",
                pattern: _adminOptions.AdminUrlPrefix + "/TenantFeatureProfiles/Edit/{name}",
                defaults: new { controller = featureProfilesControllerName, action = nameof(FeatureProfilesController.Edit) }
            );

            routes.MapAreaControllerRoute(
                name: "TenantFeatureProfilesDelete",
                areaName: "OrchardCore.Tenants",
                pattern: _adminOptions.AdminUrlPrefix + "/TenantFeatureProfiles/Delete/{name}",
                defaults: new { controller = featureProfilesControllerName, action = nameof(FeatureProfilesController.Delete) }
            );
        }
    }
}
