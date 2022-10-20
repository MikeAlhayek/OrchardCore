using System;
using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.Notifications.Models;
using OrchardCore.Users;
using OrchardCore.Users.Indexes;
using OrchardCore.Users.Models;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;
using YesSql;

namespace OrchardCore.Notifications.Activities;

public class NotifyContentOwnerTask : NotifyUserTaskActivity
{
    private readonly ISession _session;

    public NotifyContentOwnerTask(
       INotificationManager notificationCoordinator,
       IWorkflowExpressionEvaluator expressionEvaluator,
       HtmlEncoder htmlEncoder,
       ILogger<NotifyUserTask> logger,
       IStringLocalizer<NotifyUserTask> localizer,
       ISession session
   ) : base(notificationCoordinator,
       expressionEvaluator,
       htmlEncoder,
       logger,
       localizer)
    {
        _session = session;
    }

    public override string Name => nameof(NotifyContentOwnerTask);

    public override LocalizedString DisplayText => S["Notify Content's Owner Task"];

    public NotificationLinkType LinkType
    {
        get => GetProperty(() => NotificationLinkType.None);
        set => SetProperty(value);
    }

    public WorkflowExpression<string> Url
    {
        get => GetProperty(() => new WorkflowExpression<string>());
        set => SetProperty(value);
    }

    protected override async Task<IUser> GetUserAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        if (workflowContext.Input.TryGetValue("ContentItem", out var obj)
            && obj is ContentItem contentItem
            && !String.IsNullOrEmpty(contentItem.Owner))
        {
            var owner = await _session.Query<User, UserIndex>(x => x.UserId == contentItem.Owner).FirstOrDefaultAsync();

            workflowContext.Input.TryAdd("Owner", owner);

            return owner;
        }

        return null;
    }

    protected override async Task<INotificationMessage> GetMessageAsync(WorkflowExecutionContext workflowContext)
    {
        var message = new ContentNotificationMessage()
        {
            Summary = await _expressionEvaluator.EvaluateAsync(Summary, workflowContext, _htmlEncoder),
            Body = await _expressionEvaluator.EvaluateAsync(Body, workflowContext, _htmlEncoder),
            IsHtmlBody = IsHtmlBody,
            LinkType = NotificationLinkType.None,
        };

        if (LinkType == NotificationLinkType.Custom && !String.IsNullOrWhiteSpace(Url.Expression))
        {
            message.LinkType = NotificationLinkType.Custom;
            message.CustomUrl = Url.Expression;
        }

        if (workflowContext.Input.TryGetValue("ContentItem", out var obj) && obj is ContentItem contentItem)
        {
            message.ContentItemId = contentItem.ContentItemId;
            message.ContentOwnerId = contentItem.Owner;
            message.ContentType = contentItem.ContentType;
            message.LinkType = LinkType;
        }

        return message;
    }
}
