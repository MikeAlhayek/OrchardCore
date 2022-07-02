using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Apis.GraphQL
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
                .Add(S["Configuration"], NavigationConstants.AdminMenuConfigurationPosition, configuration => configuration
                    .AddClass("menu-configuration").Id("configuration")
                    .Add(S["GraphiQL"], S["GraphiQL"].PrefixPosition(), graphiQl => graphiQl
                        .Action("Index", "Admin", new { area = "OrchardCore.Apis.GraphQL" })
                        .Permission(Permissions.ExecuteGraphQL)
                        .LocalNav()
                    )
                );

            return Task.CompletedTask;
        }
    }
}
