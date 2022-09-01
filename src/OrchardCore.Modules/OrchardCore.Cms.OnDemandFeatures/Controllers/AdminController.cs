using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.Cms.OnDemandFeatures.Abstractions;
using OrchardCore.Cms.OnDemandFeatures.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Modules;
using OrchardCore.Users.Indexes;
using OrchardCore.Users.Models;
using YesSql;
using YesSql.Services;

namespace OrchardCore.Cms.OnDemandFeatures.Controllers;

[Feature("OC.Notification")]
public class AdminController : Controller, IUpdateModel
{
    private readonly INotificationCoordinator _notificationCoordinator;
    private readonly IContentManager _contentManager;
    private readonly IContentItemDisplayManager _contentItemDisplayManager;
    private readonly IAuthorizationService _authorizationService;
    private readonly ISession _session;
    private readonly INotifier _notifier;
    public readonly IHtmlLocalizer H;

    public AdminController(INotificationCoordinator notificationCoordinator,
        IContentManager contentManager,
        IContentItemDisplayManager contentItemDisplayManager,
        IAuthorizationService authorizationService,
        ISession session,
        INotifier notifier,
        IHtmlLocalizer<AdminController> htmlLocalizer)
    {
        _notificationCoordinator = notificationCoordinator;
        _contentManager = contentManager;
        _contentItemDisplayManager = contentItemDisplayManager;
        _authorizationService = authorizationService;
        _session = session;
        _notifier = notifier;
        H = htmlLocalizer;
    }

    public async Task<ActionResult> Notify()
    {
        if (!await _authorizationService.AuthorizeAsync(User, NotifyPermissions.Notify))
        {
            return Forbid();
        }

        var item = await _contentManager.NewAsync("UserNotifier");

        var shape = await _contentItemDisplayManager.BuildEditorAsync(item, this, true);

        return View(shape);
    }

    [ActionName(nameof(Notify)), HttpPost]
    public async Task<ActionResult> NotifyPOST()
    {
        if (!await _authorizationService.AuthorizeAsync(User, NotifyPermissions.Notify))
        {
            return Forbid();
        }

        var item = await _contentManager.NewAsync("UserNotifier");

        var shape = await _contentItemDisplayManager.UpdateEditorAsync(item, this, true);

        if (ModelState.IsValid)
        {
            var notifyUserPart = item.As<NotifyUserPart>();
            var body = notifyUserPart.Subject.Text;
            if (notifyUserPart != null
                && notifyUserPart.Users != null
                && notifyUserPart.Users.UserIds != null
                && notifyUserPart.Users.UserIds.Length > 0
                && !String.IsNullOrWhiteSpace(body))
            {
                var users = await _session.Query<User, UserIndex>(x => x.IsEnabled && x.UserId.IsIn(notifyUserPart.Users.UserIds)).ListAsync();
                var subject = notifyUserPart.Subject.Text;
                if (String.IsNullOrWhiteSpace(subject))
                {
                    subject = body.Substring(0, Math.Min(body.Length, 50));
                }

                var message = new NotificationMessage()
                {
                    Subject = subject,
                    TextBody = body,
                    Culture = CultureInfo.CurrentCulture
                };

                foreach (var user in users)
                {
                    await _notificationCoordinator.TrySendAsync(user, message);
                }

                await _notifier.SuccessAsync(H["Your message has been delivered."]);
            }
        }

        return View(shape);
    }
}
