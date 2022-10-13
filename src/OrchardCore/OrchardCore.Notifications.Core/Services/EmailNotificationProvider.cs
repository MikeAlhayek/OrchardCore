using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Email;
using OrchardCore.Users;
using OrchardCore.Users.Models;

namespace OrchardCore.Notifications.Services;

public class EmailNotificationProvider : INotificationMethodProvider
{
    private readonly ISmtpService _smtpService;
    private readonly IStringLocalizer S;

    public EmailNotificationProvider(ISmtpService smtpService, IStringLocalizer<EmailNotificationProvider> stringLocalizer)
    {
        _smtpService = smtpService;
        S = stringLocalizer;
        Name = S["Email Notifications"];
    }

    public string Method => "Email";

    public string Name { get; }

    public async Task<bool> TrySendAsync(IUser user, NotificationMessage message)
    {
        if (user is not User su)
        {
            return false;
        }

        var emailMessage = new MailMessage()
        {
            To = su.Email,
            Subject = message.Subject,
        };

        if (!String.IsNullOrWhiteSpace(message.HtmlBody))
        {
            emailMessage.Body = message.TextBody;
            emailMessage.IsBodyHtml = true;
        }

        if (!String.IsNullOrWhiteSpace(message.TextBody))
        {
            emailMessage.BodyText = message.TextBody;
            emailMessage.IsBodyText = true;
        }

        var result = await _smtpService.SendAsync(emailMessage);

        return result.Succeeded;
    }
}
