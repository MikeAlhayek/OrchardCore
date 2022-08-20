using System;

namespace OrchardCore.Contents.ViewModels;

public class ContentProfileSettingsViewModel
{
    public string[] ContainedContentTypes { get; set; } = Array.Empty<string>();

    public string Editor { get; set; }

    public bool DisplayAsProfile { get; set; }
}
