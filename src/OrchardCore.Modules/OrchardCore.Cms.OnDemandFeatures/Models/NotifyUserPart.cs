using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;

namespace OrchardCore.Cms.OnDemandFeatures.Models;

public class NotifyUserPart : ContentPart
{
    public UserPickerField Users { get; set; }

    public TextField Subject { get; set; }

    public TextField Body { get; set; }
}
