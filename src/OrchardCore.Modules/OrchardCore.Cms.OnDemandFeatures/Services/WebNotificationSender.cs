using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OrchardCore.Cms.OnDemandFeatures.Abstractions;
using OrchardCore.Users;

namespace OrchardCore.Cms.OnDemandFeatures.Services;

public class WebNotificationSender : INotificationSender
{
    private readonly ILogger _logger;

    public WebNotificationSender(ILogger<WebNotificationSender> logger)
    {
        _logger = logger;
    }

    public string Type => "Web";

    public string Name => "Web Notification";


    public Task<bool> TrySendAsync(IUser user, NotificationMessage message)
    {
        _logger.LogWarning("A web notification was sent to user: ", user.UserName);

        return Task.FromResult(true);
    }
}
