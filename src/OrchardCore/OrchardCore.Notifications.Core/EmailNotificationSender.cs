using System;
using System.Threading.Tasks;
using OrchardCore.Email;
using OrchardCore.Users;
using OrchardCore.Users.Models;

namespace OrchardCore.Notifications;

public class EmailNotificationSender : INotificationSender
{
    private readonly ISmtpService _smtpService;

    public EmailNotificationSender(ISmtpService smtpService)
    {
        _smtpService = smtpService;
    }

    public string Type => "Email";

    public string Name => "Email Notifications";

    public async Task<bool> TrySendAsync(IUser user, NotificationMessage message)
    {
        if (user is not User su)
        {
            return false;
        }

        // At this point we know that the user want to recived email notifications.
        var emailMessage = new MailMessage()
        {
            To = su.Email,
            Subject = message.Subject,
        };

        if (!String.IsNullOrWhiteSpace(message.TextBody))
        {
            emailMessage.BodyText = message.TextBody;
            emailMessage.IsBodyText = false;
        }

        if (!String.IsNullOrWhiteSpace(message.HtmlBody))
        {
            emailMessage.Body = message.HtmlBody;
            emailMessage.IsBodyHtml = true;
        }

        var result = await _smtpService.SendAsync(emailMessage);

        if (result.Succeeded)
        {
            return true;
        }

        // we could not send an email
        return false;
    }
}
