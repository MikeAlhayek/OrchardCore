using System.Collections.Generic;
using OrchardCore.ContentManagement;

namespace OrchardCore.Cms.OnDemandFeatures.Models;

public class UserDevicePart : ContentPart
{
    public List<UserDevice> Devices { get; set; } = new List<UserDevice>();
}
