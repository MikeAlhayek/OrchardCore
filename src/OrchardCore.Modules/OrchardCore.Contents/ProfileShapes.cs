using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Contents.Models;
using OrchardCore.DisplayManagement.Descriptors;

namespace OrchardCore.Contents;

public class ProfileShapes : IShapeTableProvider
{
    private IContentDefinitionManager _contentDefinitionManager;
    private IHttpContextAccessor _httpContextAccessor;
    public void Discover(ShapeTableBuilder builder)
    {
        builder.Describe("ContentsTitle_SummaryAdmin")
            .OnDisplaying(displaying =>
            {
                dynamic shape = displaying.Shape;

                if (shape.ContentItem is ContentItem contentItem)
                {
                    if (contentItem.Has<ContainedProfilePart>())
                    {
                        displaying.Shape.Metadata.Alternates.Add("Profile_ContentsTitle_SummaryAdmin");

                        return;
                    }

                    _contentDefinitionManager ??= displaying.ServiceProvider.GetRequiredService<IContentDefinitionManager>();

                    var definition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);

                    var profileSettings = definition.GetSettings<ContentProfileSettings>();

                    if (profileSettings.ContainedContentTypes != null && profileSettings.ContainedContentTypes.Length > 0)
                    {
                        displaying.Shape.Metadata.Alternates.Add("Profile_ContentsTitle_SummaryAdmin");
                    }
                }
            });

        builder.Describe("ContentsButtonEdit_SummaryAdmin")
           .OnDisplaying(displaying =>
           {
               dynamic shape = displaying.Shape;

               if (shape.ContentItem is ContentItem contentItem)
               {
                   if (contentItem.Has<ContainedProfilePart>())
                   {
                       displaying.Shape.Metadata.Alternates.Add("Profile_ContentsButtonEdit_SummaryAdmin");

                       return;
                   }

                   _contentDefinitionManager ??= displaying.ServiceProvider.GetRequiredService<IContentDefinitionManager>();

                   var definition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);

                   var profileSettings = definition.GetSettings<ContentProfileSettings>();

                   if (profileSettings.ContainedContentTypes != null && profileSettings.ContainedContentTypes.Length > 0)
                   {
                       displaying.Shape.Metadata.Alternates.Add("Profile_ContentsButtonEdit_SummaryAdmin");
                   }
               }
           });

        builder.Describe("ContentsAdminListCreate")
           .OnDisplaying(displaying =>
           {
               _httpContextAccessor ??= displaying.ServiceProvider.GetService<IHttpContextAccessor>();
               var feature = _httpContextAccessor.HttpContext.Features.Get<ContentProfileFeature>();

               if (feature != null)
               {
                   displaying.Shape.Metadata.Alternates.Add("Profile_ContentsAdminListCreate");
               }
           });
    }
}
