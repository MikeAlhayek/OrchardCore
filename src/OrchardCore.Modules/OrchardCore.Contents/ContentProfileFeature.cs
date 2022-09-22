using OrchardCore.ContentManagement;
using OrchardCore.Contents.Models;

namespace OrchardCore.Contents;

public class ContentProfileFeature
{
    public ContentItem ProfileContentItem { get; set; }

    public ContentProfileSettings ContentProfileSettings { get; set; }
}
