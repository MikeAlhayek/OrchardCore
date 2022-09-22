using System;

namespace OrchardCore.Contents.Models;

public class ContentProfileSettings
{
    public string[] ContainedContentTypes { get; set; } = Array.Empty<string>();

    public string DisplayMode { get; set; }
}
