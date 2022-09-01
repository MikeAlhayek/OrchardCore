using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Cms.OnDemandFeatures.Abstractions;
using OrchardCore.Cms.OnDemandFeatures.Models;
using OrchardCore.Entities;
using OrchardCore.Users;
using OrchardCore.Users.Models;

namespace OrchardCore.Cms.OnDemandFeatures.Services;

public class PushNotificationSender : INotificationSender
{
    private readonly IPushNotificationService _pushNotificationService;

    public PushNotificationSender(IPushNotificationService pushNotificationService)
    {
        _pushNotificationService = pushNotificationService;
    }

    public const string TheKey = "Push";

    public string Type => TheKey;

    public string Name => "Push Notifications";

    public async Task<bool> TrySendAsync(IUser user, NotificationMessage message)
    {
        if (user is not User su)
        {
            return false;
        }

        var devicePart = su.As<UserDevicePart>();

        if (devicePart != null && devicePart.Devices != null)
        {
            // get the device ids from the user profile
            var deviceIds = devicePart.Devices
                .OrderByDescending(x => x.LastUsedUtcAt)
                .Select(x => x.DeviceId)
                .ToArray();

            if (deviceIds.Length > 0)
            {
                var totalSent = await _pushNotificationService.PushAsync(deviceIds, message);

                // at least one notification was sent
                return totalSent > 0;
            }
        }

        // we could not send notification
        return false;
    }
}
