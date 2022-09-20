using System;

namespace OrchardCore.Contents.Models;

public class ContentProfileSettings
{
    public string[] ContainedContentTypes { get; set; } = Array.Empty<string>();

    public string Editor { get; set; }
}
