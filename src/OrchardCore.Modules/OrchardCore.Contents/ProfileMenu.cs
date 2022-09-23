using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Utilities;
using OrchardCore.Contents.Models;
using OrchardCore.Navigation;

namespace OrchardCore.Contents;

public class StandardProfileMenu : INavigationProvider
{
    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IStringLocalizer S;

    public StandardProfileMenu(IContentDefinitionManager contentDefinitionManager,
        IHttpContextAccessor httpContextAccessor,
        IStringLocalizer<AdminMenu> localizer)
    {
        _contentDefinitionManager = contentDefinitionManager;
        _httpContextAccessor = httpContextAccessor;
        S = localizer;
    }

    public Task BuildNavigationAsync(string name, NavigationBuilder builder)
    {
        if (!name.StartsWith("Profile."))
        {
            return Task.CompletedTask;
        }

        var contentType = name.Replace("Profile.", "");

        var definition = _contentDefinitionManager.GetTypeDefinition(contentType);

        var profileSettings = definition.GetSettings<ContentProfileSettings>();

        if (profileSettings == null)
        {
            return Task.CompletedTask;
        }

        var profileFeature = _httpContextAccessor.HttpContext.Features.Get<ContentProfileFeature>();

        if (profileFeature?.ContentProfileSettings == null
            || !String.IsNullOrEmpty(profileFeature.ContentProfileSettings.DisplayMode))
        {
            return Task.CompletedTask;
        }

        var profileDisplayName = definition.DisplayName ?? definition.Name.CamelFriendly();

        builder
            .Add(S["Edit {0}", profileDisplayName], "10", edit => edit
                    .Action("Edit", "Profile", new { area = "OrchardCore.Contents", profileId = profileFeature.ProfileContentItem.ContentItemId, contentItemId = String.Empty })
                    .Permission(CommonPermissions.EditContent)
                    .Resource(profileFeature.ProfileContentItem)
                )
            .Add(S["View {0}", profileDisplayName], "20", display => display
                .Action("Display", "Profile", new { area = "OrchardCore.Contents", profileId = profileFeature.ProfileContentItem.ContentItemId, contentItemId = String.Empty })
                .Permission(CommonPermissions.ViewContent)
                .Resource(profileFeature.ProfileContentItem)
            );

        if (profileFeature.ContentProfileSettings.ContainedContentTypes.Length > 0)
        {
            builder
                .Add(S["Manage Content"], "30", edit => edit
                    .Action("List", "Profile", new { area = "OrchardCore.Contents", profileId = profileFeature.ProfileContentItem.ContentItemId })
                    .Permission(CommonPermissions.EditContent)
                    .Resource(profileFeature.ProfileContentItem)
                );
        }

        return Task.CompletedTask;
    }
}
