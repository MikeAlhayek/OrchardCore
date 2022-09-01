using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Cms.OnDemandFeatures.Controllers;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Navigation;

namespace OrchardCore.Cms.OnDemandFeatures;

public class AdminMenu : INavigationProvider
{
    private readonly IStringLocalizer S;

    public AdminMenu(IStringLocalizer<AdminMenu> localizer)
    {
        S = localizer;
    }

    public Task BuildNavigationAsync(string name, NavigationBuilder builder)
    {
        if (!String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
        {
            return Task.CompletedTask;
        }

        builder
            .Add(S["Examples"], S["Examples"].PrefixPosition(), example => example
                .Add(S["Notify User"], S["Notify User"].PrefixPosition(), notifiyUser => notifiyUser
                    .AddClass("Examples_Notify_User")
                    .Id("Examples_Notify_User")
                    .Action(nameof(AdminController.Notify), typeof(AdminController).ControllerName(), "OrchardCore.Cms.OnDemandFeatures")
                    .Permission(NotifyPermissions.Notify)
                    .LocalNav()
                )
            );

        return Task.CompletedTask;
    }
}
