using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.ReverseProxy
{
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
                .Add(S["Configuration"], configuration => configuration
                    .Add(S["Settings"], settings => settings
                        .Add(S["Reverse Proxy"], S["Reverse Proxy"].PrefixPosition(), proxy => proxy
                            .AddClass("reverseproxy").Id("reverseproxy")
                            .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = "ReverseProxy" })
                            .Permission(Permissions.ManageReverseProxySettings)
                            .LocalNav()
                        )
                    )
                );

            return Task.CompletedTask;
        }
    }
}
