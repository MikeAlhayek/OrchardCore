using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Contents.Models;
using OrchardCore.DisplayManagement.Descriptors;

namespace OrchardCore.Contents;

public class ProfileShapes : IShapeTableProvider
{
    private IContentDefinitionManager contentDefinitionManager;

    public void Discover(ShapeTableBuilder builder)
    {
        builder.Describe("ContentsTitle_SummaryAdmin")
               .OnDisplaying(displaying =>
               {
                   dynamic shape = displaying.Shape;

                   if (shape.ContentItem is ContentItem contentItem)
                   {
                       contentDefinitionManager ??= displaying.ServiceProvider.GetRequiredService<IContentDefinitionManager>();

                       var definition = contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);

                       var profileSettings = definition.GetSettings<ContentProfileSettings>();

                       if (contentItem.ContentType == "Client")
                       {
                           profileSettings = new ContentProfileSettings()
                           {
                               ContainedContentTypes = new[] { "ClientLocation" },
                           };
                       }

                       if (profileSettings.ContainedContentTypes != null && profileSettings.ContainedContentTypes.Length > 0)
                       {
                           displaying.Shape.Metadata.Alternates.Add("Profile_ContentsTitle_SummaryAdmin");
                       }
                   }
               });
    }
}
