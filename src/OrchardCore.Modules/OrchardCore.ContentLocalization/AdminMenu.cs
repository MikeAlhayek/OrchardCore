using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentLocalization.Drivers;
using OrchardCore.Modules;
using OrchardCore.Navigation;

namespace OrchardCore.ContentLocalization
{
    [Feature("OrchardCore.ContentLocalization.ContentCulturePicker")]
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
                        .Add(S["Localization"], localization => localization
                            .Add(S["Content request culture provider"], S["Content request culture provider"].PrefixPosition(), provider => provider
                                .AddClass("contentrequestcultureprovider").Id("contentrequestcultureprovider")
                                .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = ContentRequestCultureProviderSettingsDriver.GroupId })
                                .Permission(Permissions.ManageContentCulturePicker)
                                .LocalNav()
                            )
                            .Add(S["Content culture picker"], S["Content culture picker"].PrefixPosition(), picker => picker
                                .AddClass("contentculturepicker").Id("contentculturepicker")
                                .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = ContentCulturePickerSettingsDriver.GroupId })
                                .Permission(Permissions.ManageContentCulturePicker)
                                .LocalNav()
                            )
                        )
                    )
                );

            return Task.CompletedTask;
        }
    }
}
