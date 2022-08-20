using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.Contents.ViewModels;

public class ProfileViewModel
{
    public dynamic Header { get; set; }

    public dynamic Navigation { get; set; }

    public dynamic Body { get; set; }

    public ContentItem ProfileContentItem { get; set; }

    public ContentItem ContentItem { get; set; }

    public ContentTypeDefinition ContentType { get; set; }
}
