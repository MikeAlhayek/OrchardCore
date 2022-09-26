using System;
using System.Collections.Generic;
using System.Linq;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Contents.Models;

namespace OrchardCore.Contents.Services;

public class ProfileContentTypeProvider
{
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public ProfileContentTypeProvider(IContentDefinitionManager contentDefinitionManager)
    {
        _contentDefinitionManager = contentDefinitionManager;
    }

    public IEnumerable<string> GetAllContainedInProfileContentTypeNames()
    {
        var contentTypeDefinitions = new List<string>();

        var definitions = _contentDefinitionManager.ListTypeDefinitions();

        foreach (var definition in definitions)
        {
            var settings = definition.GetSettings<ContentProfileSettings>();

            if (settings.ContainedContentTypes != null)
            {
                contentTypeDefinitions.AddRange(settings.ContainedContentTypes);
            }
        }

        return contentTypeDefinitions;
    }

    public ContentTypeDefinition GetProfileType(string containedContentType)
    {
        var definitions = _contentDefinitionManager.ListTypeDefinitions();

        foreach (var definition in definitions)
        {
            var settings = definition.GetSettings<ContentProfileSettings>();

            if (settings.ContainedContentTypes != null && settings.ContainedContentTypes.Contains(containedContentType, StringComparer.OrdinalIgnoreCase))
            {
                return definition;
            }
        }

        return null;
    }
}
