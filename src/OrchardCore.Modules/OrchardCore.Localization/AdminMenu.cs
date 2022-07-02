using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Localization.Drivers;
using OrchardCore.Navigation;

namespace OrchardCore.Localization
{
    /// <summary>
    /// Represents a localization menu in the admin site.
    /// </summary>
    public class AdminMenu : INavigationProvider
    {
        private readonly IStringLocalizer S;

        /// <summary>
        /// Creates a new instance of the <see cref="AdminMenu"/>.
        /// </summary>
        /// <param name="localizer">The <see cref="IStringLocalizer"/>.</param>
        public AdminMenu(IStringLocalizer<AdminMenu> localizer)
        {
            S = localizer;
        }

        ///<inheritdocs />
        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (!String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return Task.CompletedTask;
            }

            builder
                .Add(S["Configuration"], NavigationConstants.AdminMenuConfigurationPosition, configuration => configuration
                    .Add(S["Settings"], settings => settings
                        .Add(S["Localization"], localization => localization
                            .AddClass("localization").Id("localization")
                            .Add(S["Cultures"], S["Cultures"].PrefixPosition(), cultures => cultures
                                .AddClass("cultures").Id("cultures")
                                .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = LocalizationSettingsDisplayDriver.GroupId })
                                .Permission(Permissions.ManageCultures)
                                .LocalNav()
                            )
                        )
                    )
                );

            return Task.CompletedTask;
        }
    }
}
