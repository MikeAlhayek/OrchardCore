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
    private readonly IShortcodeService _shortcodeService;
    private readonly HtmlEncoder _htmlEncoder;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IServiceProvider _serviceProvider;
    private UserManager<IUser> _userManager;

    public NotificationMessageProvider(YesSql.ISession session,
        ILiquidTemplateManager liquidTemplateManager,
        IShortcodeService shortcodeService,
        HtmlEncoder htmlEncoder,
        IHttpContextAccessor httpContextAccessor,
        IServiceProvider serviceProvider)
    {
        _session = session;
        _liquidTemplateManager = liquidTemplateManager;
        _shortcodeService = shortcodeService;
        _htmlEncoder = htmlEncoder;
        _httpContextAccessor = httpContextAccessor;
        _serviceProvider = serviceProvider;
    }

    public async Task<IEnumerable<NotificationMessageContext>> GetAsync(string template, Dictionary<string, string> specificArguments)
    {
        if (String.IsNullOrWhiteSpace(template))
        {
            throw new ArgumentException($"{nameof(template)} cannot be empty.");
        }
        var templateItems = await _session.Query<ContentItem, NotificationTemplateIndex>(x => x.TemplateName == template).ListAsync();

        var arguments = new Dictionary<string, FluidValue>();

        foreach (var argument in specificArguments)
        {
            arguments.TryAdd(argument.Key, new ObjectValue(argument.Value));
        }

        var messages = new List<NotificationMessageContext>();

        var userIds = new List<string>();

        foreach (var templateItem in templateItems)
        {
            var deliveryTo = templateItem.As<NotificationTemplateDeliveryPart>();

            if (NotificationTemplateConstants.SpecificUsersValue.Equals(deliveryTo.SendTo?.Text))
            {
                userIds.AddRange(deliveryTo.Users.UserIds);
            }
        }

        var users = await _session.Query<User, UserIndex>(x => x.UserId.IsIn(userIds)).ListAsync();

        IUser user = null;

        if (_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
        {
            // resolve userManager from the serviceProvider to eliminate circular dependency
            _userManager ??= _serviceProvider.GetRequiredService<UserManager<IUser>>();
            user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
        }

        foreach (var templateItem in templateItems)
        {
            var templatePart = templateItem.As<NotificationMessageTemplatePart>();

            var body = new NotificationMessageContext()
            {
                Subject = await GetTextBodyAsync(arguments, templateItem, templatePart.Subject.Text),
                TextBody = await GetTextBodyAsync(arguments, templateItem, templatePart.Body.Markdown),
            };

            var deliveryToPart = templateItem.As<NotificationTemplateDeliveryPart>();

            if (NotificationTemplateConstants.CurrentUserValue.Equals(deliveryToPart.SendTo?.Text) && user != null)
            {
                body.Users.Add(user);
            }
            else if (NotificationTemplateConstants.SpecificUsersValue.Equals(deliveryToPart.SendTo?.Text))
            {
                body.Users.AddRange(users.Where(x => deliveryToPart.Users.UserIds.Contains(x.UserId)));
            }

            messages.Add(body);
        }

        return messages;
    }

    private async Task<string> GetTextBodyAsync(Dictionary<string, FluidValue> arguments, ContentItem templateItem, string message)
    {
        var body = await _liquidTemplateManager.RenderStringAsync(message, _htmlEncoder, null, arguments);

        body = await _shortcodeService.ProcessAsync(body,
              new Context
              {
                  ["ContentItem"] = templateItem,
              });

        return body;
    }
}
