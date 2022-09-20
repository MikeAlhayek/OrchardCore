using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Contents.Controllers;
using OrchardCore.Contents.Indexing;
using OrchardCore.Contents.ViewModels;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Mvc.Core.Utilities;
using YesSql;
using YesSql.Services;

namespace OrchardCore.Contents.Services;

public class ProfileContentTypeAdminListFilter : IContentsAdminListFilter
{
    private readonly YesSql.ISession _session;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ProfileContentTypeAdminListFilter(YesSql.ISession session, IHttpContextAccessor httpContextAccessor)
    {
        _session = session;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task FilterAsync(ContentOptionsViewModel model, IQuery<ContentItem> query, IUpdateModel updater)
    {
        var controllerName = _httpContextAccessor.HttpContext.Request.RouteValues["Controller"]?.ToString();
        var actionName = _httpContextAccessor.HttpContext.Request.RouteValues["Action"]?.ToString();

        if (String.Equals(typeof(ProfileController).ControllerName(), controllerName, StringComparison.OrdinalIgnoreCase)
            && String.Equals(nameof(ProfileController.List), actionName, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        // TODO, find a way to reduce the round trip by using a single query instead
        var excludedIds = (await _session.QueryIndex<ContainedProfilePartIndex>().ListAsync())
            .Select(x => x.ContentItemId);

        query.With<ContentItemIndex>(x => x.ContentItemId.IsNotIn(excludedIds));
    }
}
