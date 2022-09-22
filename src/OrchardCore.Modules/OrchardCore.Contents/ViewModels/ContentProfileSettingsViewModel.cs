using System;

namespace OrchardCore.Contents.ViewModels;

public class ContentProfileSettingsViewModel
{
    public string[] ContainedContentTypes { get; set; } = Array.Empty<string>();

    public string DisplayMode { get; set; }

    public bool DisplayAsProfile { get; set; }
}
