using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Cms.OnDemandFeatures.Abstractions;

namespace OrchardCore.Cms.OnDemandFeatures;

public interface IPushNotificationService
{
    Task<int> PushAsync(IEnumerable<string> deviceIds, NotificationMessage message);
    Task<bool> TryPushAsync(string deviceId, NotificationMessage message);
}
