using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Entities;
using OrchardCore.Notifications.Abstractions;
using OrchardCore.Users;
using OrchardCore.Users.Models;

namespace OrchardCore.Notifications.Core;

public class NotificationManager : INotificationManager
{
    private readonly IEnumerable<INotificationSender> _senders;
    private readonly NotificationOptions _notificationCoordinatorOptions;
    private readonly ILogger _logger;

    public NotificationManager(IEnumerable<INotificationSender> senders,
        IOptions<NotificationOptions> notificationCoordinatorOptions,
        ILogger<NotificationManager> logger)
    {
        _senders = senders;
        _notificationCoordinatorOptions = notificationCoordinatorOptions.Value;
        _logger = logger;
    }

    public async Task<int> TrySendAsync(IUser user, NotificationMessage message)
    {
        if (user is not User su)
        {
            return 0;
        }

        var notificationPart = su.As<UserNotificationPart>();

        // here we attempt to send the notification top to bottom as the priority matters in this case
        var selectedTypes = (notificationPart?.Types) ?? Array.Empty<string>();

        foreach (var selectedType in selectedTypes)
        {
            var sender = _senders.FirstOrDefault(s => String.Equals(s.Type, selectedType, StringComparison.OrdinalIgnoreCase));

            if (sender == null)
            {
                _logger.LogWarning($"No {nameof(INotificationSender)} to handle type {0}", selectedType);
                continue;
            }

            if (_notificationCoordinatorOptions.Strategy == NotificationStrategy.NotifyFirstSuccess)
            {
                if (await sender.TrySendAsync(user, message))
                {
                    // since one notification was sent, we no longer need to send using other services
                    return 1;
                }
            }
            else
            {
                await sender.TrySendAsync(user, message);
            }
        }

        // we could not send an email
        return 0;
    }
}
