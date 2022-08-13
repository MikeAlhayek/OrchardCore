using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ViewModels;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Contents.Models;
using OrchardCore.DisplayManagement;
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
                   var shape = displaying.Shape;
                   contentDefinitionManager ??= displaying.ServiceProvider.GetRequiredService<IContentDefinitionManager>();

                   if (displaying.Shape.TryGetProperty(nameof(ContentItemViewModel.ContentItem), out ContentItem contentItem))
                   {
                       var definition = contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);
                       var settings = definition.GetSettings<ContentProfileSettings>();

                       if (settings.ContainedContentTypes != null && settings.ContainedContentTypes.Length > 0)
                       {
                           displaying.Shape.Metadata.Alternates.Add("Profile_ContentsTitle_SummaryAdmin");
                       }
                   }
               });
    }
}
