using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Facebook
{
    public class AdminMenu : INavigationProvider
    {
        private readonly IStringLocalizer S;

        public AdminMenu(
            IStringLocalizer<AdminMenu> localizer)
        {
            S = localizer;
        }

        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                builder.Add(S["Configuration"], configuration => configuration
                        .Add(S["Settings"], settings => settings
                            .Add(S["Facebook App"], S["Facebook App"].PrefixPosition(), settings => settings
                            .AddClass("facebookApp").Id("facebookApp")
                            .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = FacebookConstants.Features.Core })
                            .Permission(Permissions.ManageFacebookApp)
                            .LocalNav())
                        )
                    );
            }
            return Task.CompletedTask;
        }
    }

    public class AdminMenuLogin : INavigationProvider
    {
        private readonly IStringLocalizer S;

        public AdminMenuLogin(
            IStringLocalizer<AdminMenuLogin> localizer)
        {
            S = localizer;
        }

        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                builder.Add(S["Security"], security => security
                        .Add(S["Authentication"], authentication => authentication
                        .Add(S["Facebook"], S["Facebook"].PrefixPosition(), settings => settings
                        .AddClass("facebook").Id("facebook")
                            .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = FacebookConstants.Features.Login })
                            .Permission(Permissions.ManageFacebookApp)
                            .LocalNav())
                ));
            }

            return Task.CompletedTask;
        }
    }

    public class AdminMenuPixel : INavigationProvider
    {
        private readonly IStringLocalizer S;

        public AdminMenuPixel(
            IStringLocalizer<AdminMenuLogin> stringLocalizer)
        {
            S = stringLocalizer;
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
                        .Add(S["Facebook Pixel"], S["Facebook Pixel"].PrefixPosition(), pixel => pixel
                            .AddClass("facebookPixel").Id("facebookPixel")
                            .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = FacebookConstants.PixelSettingsGroupId })
                            .Permission(Permissions.ManageFacebookApp)
                            .LocalNav()
                        )
                    )
                );

            return Task.CompletedTask;
        }
    }
}
