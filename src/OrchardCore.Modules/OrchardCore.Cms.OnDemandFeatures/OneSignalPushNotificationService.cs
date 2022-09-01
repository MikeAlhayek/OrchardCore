using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OrchardCore.Cms.OnDemandFeatures.Abstractions;

namespace OrchardCore.Cms.OnDemandFeatures;

public class OneSignalPushNotificationService : IPushNotificationService
{
    private readonly ILogger _logger;

    public OneSignalPushNotificationService(ILogger<OneSignalPushNotificationService> logger)
    {
        _logger = logger;
    }

    public Task<int> PushAsync(IEnumerable<string> deviceIds, NotificationMessage message)
    {
        _logger.LogWarning("A message was sent using OneSignal Push Notification to deviceIds:", String.Join(',', deviceIds ?? Array.Empty<string>()));

        return Task.FromResult(deviceIds.Count());
    }

    public Task<bool> TryPushAsync(string deviceId, NotificationMessage message)
    {
        _logger.LogWarning("A message was sent using OneSignal Push Notification to deviceId: ", deviceId);

        return Task.FromResult(true);
    }
}
