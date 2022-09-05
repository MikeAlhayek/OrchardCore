using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid.Values;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.Liquid;
using OrchardCore.Notifications.Models;
using OrchardCore.Shortcodes.Services;
using OrchardCore.Users;
using OrchardCore.Users.Indexes;
using OrchardCore.Users.Models;
using Shortcodes;
using YesSql;
using YesSql.Services;

namespace OrchardCore.Notifications;

public class NotificationMessageProvider : INotificationMessageProvider
{
    private readonly YesSql.ISession _session;
    private readonly ILiquidTemplateManager _liquidTemplateManager;
    private readonly HtmlEncoder _htmlEncoder;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IServiceProvider _serviceProvider;

    private IShortcodeService _shortcodeService;
    private UserManager<IUser> _userManager;

    public NotificationMessageProvider(YesSql.ISession session,
        ILiquidTemplateManager liquidTemplateManager,
        HtmlEncoder htmlEncoder,
        IHttpContextAccessor httpContextAccessor,
        IServiceProvider serviceProvider)
    {
        _session = session;
        _liquidTemplateManager = liquidTemplateManager;
        _htmlEncoder = htmlEncoder;
        _httpContextAccessor = httpContextAccessor;
        _serviceProvider = serviceProvider;
    }

    public async Task<IEnumerable<NotificationMessageContext>> GetAsync(string template, Dictionary<string, string> messageArguments)
    {
        if (String.IsNullOrWhiteSpace(template))
        {
            throw new ArgumentException($"{nameof(template)} cannot be empty.");
        }

        var templateItems = await _session.Query<ContentItem, NotificationTemplateIndex>(x => x.TemplateName == template).ListAsync();

        return await GetAsync(templateItems, messageArguments);
    }

    public async Task<IEnumerable<NotificationMessageContext>> GetAsync(IEnumerable<ContentItem> templates, Dictionary<string, string> messageArguments)
    {
        if (templates == null)
        {
            throw new ArgumentNullException(nameof(templates));
        }

        var arguments = new Dictionary<string, FluidValue>();

        foreach (var messageArgument in messageArguments)
        {
            arguments.TryAdd(messageArgument.Key, new ObjectValue(messageArgument.Value));
        }

        var messages = new List<NotificationMessageContext>();

        var userIds = new List<string>();

        foreach (var templateItem in templates)
        {
            var deliveryTo = templateItem.As<NotificationReceiverPart>();

            if (deliveryTo == null || deliveryTo.Receivers == null)
            {
                continue;
            }

            if (deliveryTo.Receivers.Contains(NotificationTemplateConstants.SpecificUsersValue, StringComparer.OrdinalIgnoreCase))
            {
                var usersPart = templateItem.As<NotificationReceivingUsersPart>();

                if (usersPart == null || usersPart.Users == null)
                {
                    continue;
                }

                userIds.AddRange(usersPart.Users.UserIds);
            }
        }

        //usersPart

        var users = await _session.Query<User, UserIndex>(x => x.UserId.IsIn(userIds)).ListAsync();

        IUser user = null;

        if (_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
        {
            // resolve userManager from the serviceProvider to eliminate circular dependency
            _userManager ??= _serviceProvider.GetRequiredService<UserManager<IUser>>();
            user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
        }

        foreach (var template in templates)
        {
            var templatePart = template.As<NotificationMessageTemplatePart>();

            var body = new NotificationMessageContext()
            {
                Subject = await GetTextBodyAsync(arguments, template, templatePart.Subject.Text),
                TextBody = await GetTextBodyAsync(arguments, template, templatePart.Body.Markdown),
            };

            var deliveryTo = template.As<NotificationReceiverPart>();

            if (deliveryTo == null || deliveryTo.Receivers == null)
            {
                continue;
            }

            if (deliveryTo.Receivers.Contains(NotificationTemplateConstants.SpecificUsersValue, StringComparer.OrdinalIgnoreCase))
            {
                body.Users.Add(user);
            }
            else if (deliveryTo.Receivers.Contains(NotificationTemplateConstants.SpecificUsersValue, StringComparer.OrdinalIgnoreCase))
            {
                var usersPart = template.As<NotificationReceivingUsersPart>();

                if (usersPart == null || usersPart.Users == null)
                {
                    continue;
                }

                body.Users.AddRange(users.Where(x => usersPart.Users.UserIds.Contains(x.UserId)));
            }

            messages.Add(body);
        }

        return messages;
    }

    private async Task<string> GetTextBodyAsync(Dictionary<string, FluidValue> arguments, ContentItem templateItem, string message)
    {
        var body = await _liquidTemplateManager.RenderStringAsync(message, _htmlEncoder, null, arguments);

        // notifications do not requires Shortcodes, but if the tenant support Shortcode, we'll apply them as well
        _shortcodeService ??= _serviceProvider.GetService<IShortcodeService>();

        if (_shortcodeService != null)
        {
            return await _shortcodeService.ProcessAsync(body,
                  new Context
                  {
                      ["ContentItem"] = templateItem,
                  });
        }

        return body;
    }
}
