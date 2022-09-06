using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OrchardCore.Modules;
using OrchardCore.Users;

namespace OrchardCore.Notifications;

public class DefaultTemplateDispatcher : INotificationTemplateDispatcher
{
    private readonly IEnumerable<INotificationSender> _notificationSenders;
    private readonly INotificationMessageProvider _notificationMessageProvider;
    private readonly ILogger _logger;

    public DefaultTemplateDispatcher(IEnumerable<INotificationSender> notificationSenders,
       INotificationMessageProvider notificationMessageProvider,
       ILogger<DefaultTemplateDispatcher> logger)
    {
        _notificationSenders = notificationSenders;
        _notificationMessageProvider = notificationMessageProvider;
        _logger = logger;
    }

    public async Task SendAsync(string template, IUser user, Dictionary<string, string> arguments, object resource)
    {
        if (String.IsNullOrWhiteSpace(template))
        {
            throw new ArgumentException($"{nameof(template)} cannot be empty.");
        }

        var messages = await _notificationMessageProvider.GetAsync(template, arguments, resource);

        foreach (var message in messages)
        {
            await _notificationSenders.InvokeAsync(async sender => await sender.TrySendAsync(user, message), _logger);
        }
    }
}
