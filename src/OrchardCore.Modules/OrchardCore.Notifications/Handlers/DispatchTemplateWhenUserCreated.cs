using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OrchardCore.Email;
using OrchardCore.Users.Handlers;
using OrchardCore.Users.Models;

namespace OrchardCore.Notifications.Handlers;

public class DispatchTemplateWhenUserCreated : UserEventHandlerBase
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly INotificationMessageProvider _notificationMessageProvider;
    private readonly ISmtpService _smtpService;

    public DispatchTemplateWhenUserCreated(IHttpContextAccessor httpContextAccessor,
        INotificationMessageProvider notificationMessageProvider,
        ISmtpService smtpService)
    {
        _httpContextAccessor = httpContextAccessor;
        _notificationMessageProvider = notificationMessageProvider;
        _smtpService = smtpService;
    }

    public override async Task CreatedAsync(UserCreateContext context)
    {
        if (_httpContextAccessor == null || _httpContextAccessor.HttpContext == null
            || !_httpContextAccessor.HttpContext.Request.Form.TryGetValue("User.Password", out var password))
        {
            return;
        }

        if (context.User is not User user)
        {
            return;
        }

        var messages = await _notificationMessageProvider.GetAsync(NewUserCreatedNotificationTemplateProvider.Key, new()
        {
            { "password", password },
            { "username", user.UserName }
        });

        foreach (var message in messages)
        {
            var mailMessage = new MailMessage()
            {
                To = user.Email,
                Subject = message.Subject,
            };

            if (!string.IsNullOrEmpty(message.HtmlBody))
            {
                mailMessage.IsBodyHtml = true;
                mailMessage.Body = message.HtmlBody;
            }

            if (!string.IsNullOrEmpty(message.TextBody))
            {
                mailMessage.IsBodyText = true;
                mailMessage.BodyText = message.TextBody;
            }

            await _smtpService.SendAsync(mailMessage);
        }
    }
}
