using System.Security.Claims;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Types;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Contents;
using OrchardCore.Contents.Security;
using YesSql;

namespace OrchardCore.ContentManagement.GraphQL.Queries;

public class ContentItemFilters : GraphQLFilter<ContentItem>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly IAuthorizationService _authorizationService;
    private readonly IContentManager _contentManager;

    public ContentItemFilters(IHttpContextAccessor httpContextAccessor,
        IContentDefinitionManager contentDefinitionManager,
        IAuthorizationService authorizationService,
        IContentManager contentManager)
    {
        _httpContextAccessor = httpContextAccessor;
        _contentDefinitionManager = contentDefinitionManager;
        _authorizationService = authorizationService;
        _contentManager = contentManager;
    }

    public override async Task<IQuery<ContentItem>> PreQueryAsync(IQuery<ContentItem> query, IResolveFieldContext context)
    {
        var contentType = ((ListGraphType)(context.FieldDefinition).ResolvedType).ResolvedType.Name;

        var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(contentType);
        var contentTypePermission = ContentTypePermissionsHelper.ConvertToDynamicPermission(CommonPermissions.ViewContent);
        var dynamicPermission = ContentTypePermissionsHelper.CreateDynamicPermission(contentTypePermission, contentTypeDefinition);

        var dummy = await _contentManager.NewAsync(contentTypeDefinition.Name);

        if (await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, dynamicPermission, dummy))
        {
            return await base.PreQueryAsync(query, context);
        }

        var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        dummy.Owner = userId;

        var contentTypeOwnPermission = ContentTypePermissionsHelper.ConvertToDynamicPermission(CommonPermissions.ViewOwnContent);

        if (await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, contentTypeOwnPermission, dummy))
        {
            return query.With<ContentItemIndex>(x => x.Owner == userId);
        }

        return query.With<ContentItemIndex>(x => x.ContentType != contentType);
    }
}
