using System;

namespace OrchardCore.Cms.OnDemandFeatures.Models;

public class UserDevice
{
    public string DeviceId { get; set; }

    public DateTime RegistredUtcAt { get; set; }

    public DateTime LastUsedUtcAt { get; set; }
}
