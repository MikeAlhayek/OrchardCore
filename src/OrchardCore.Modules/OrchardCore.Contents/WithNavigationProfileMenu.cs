using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.ContentManagement.Utilities;
using OrchardCore.Contents.Models;
using OrchardCore.Navigation;

namespace OrchardCore.Contents;

public class WithNavigationProfileMenu : INavigationProvider
{
    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IContentManager _contentManager;
    private readonly IStringLocalizer S;

    public WithNavigationProfileMenu(IContentDefinitionManager contentDefinitionManager,
        IHttpContextAccessor httpContextAccessor,
        IContentManager contentManager,
        IStringLocalizer<AdminMenu> localizer)
    {
        _contentDefinitionManager = contentDefinitionManager;
        _httpContextAccessor = httpContextAccessor;
        _contentManager = contentManager;
        S = localizer;
    }

    public async Task BuildNavigationAsync(string name, NavigationBuilder builder)
    {
        if (!name.StartsWith("Profile."))
        {
            return;
        }

        var contentType = name.Replace("Profile.", "");

        var definition = _contentDefinitionManager.GetTypeDefinition(contentType);

        var profileSettings = definition.GetSettings<ContentProfileSettings>();

        if (profileSettings == null)
        {
            return;
        }

        var profileFeature = _httpContextAccessor.HttpContext.Features.Get<ContentProfileFeature>();

        if (profileFeature?.ContentProfileSettings == null
            || !String.Equals("WithNavigation", profileFeature.ContentProfileSettings.DisplayMode, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var profileDisplayName = definition.DisplayName ?? definition.Name.CamelFriendly();

        // At this point we create a menu for any other DisplayMode
        builder
            .Add(S[profileDisplayName], profile => profile
                .Add(S["Edit"], S["Edit"].PrefixPosition(), edit => edit
                    .Action("Edit", "Profile", new { area = "OrchardCore.Contents", profileId = profileFeature.ProfileContentItem.ContentItemId, contentItemId = String.Empty })
                    .Permission(CommonPermissions.EditContent)
                    .Resource(profileFeature.ProfileContentItem)
                )
                .Add(S["View"], S["View"].PrefixPosition(), display => display
                    .Action("Display", "Profile", new { area = "OrchardCore.Contents", profileId = profileFeature.ProfileContentItem.ContentItemId, contentItemId = String.Empty })
                    .Permission(CommonPermissions.ViewContent)
                    .Resource(profileFeature.ProfileContentItem)
                )
         );

        foreach (var containedContentType in profileSettings.ContainedContentTypes)
        {
            var containedType = _contentDefinitionManager.GetTypeDefinition(containedContentType);
            var typeDisplayName = containedType.DisplayName ?? definition.Name.CamelFriendly();
            var dummyContainedItem = await _contentManager.NewAsync(containedContentType);
            var settings = containedType.GetSettings<ContentTypeSettings>();

            if (settings.Listable)
            {
                builder
                    .Add(S[typeDisplayName], type => type
                        .Add(S["Manage"], S["Manage"].PrefixPosition(), list => list
                            .Action("List", "Profile", new { area = "OrchardCore.Contents", profileId = profileFeature.ProfileContentItem.ContentItemId, contentTypeId = containedContentType })
                            .Permission(CommonPermissions.ViewContent)
                            .Resource(dummyContainedItem)
                        )
                    );
            }

            if (settings.Creatable)
            {
                builder
                    .Add(S[typeDisplayName], type => type
                        .Add(S["New"], S["New"].PrefixPosition(), list => list
                            .Action("Create", "Profile", new { area = "OrchardCore.Contents", profileId = profileFeature.ProfileContentItem.ContentItemId, contentTypeId = containedContentType })
                            .Permission(CommonPermissions.EditContent)
                            .Resource(dummyContainedItem)
                            .LocalNav()
                        )
                    );
            }
        }
    }
}
