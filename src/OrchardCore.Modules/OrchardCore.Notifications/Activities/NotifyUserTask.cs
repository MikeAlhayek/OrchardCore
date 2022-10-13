using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.Users;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Notifications.Activities;


public class NotifyUserTask : TaskActivity
{
    private readonly INotificationManager _notificationCoordinator;
    private readonly IWorkflowExpressionEvaluator _expressionEvaluator;
    private readonly IStringLocalizer S;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly HtmlEncoder _htmlEncoder;
    private readonly ILogger _logger;

    public NotifyUserTask(
        INotificationManager notificationCoordinator,
        IWorkflowExpressionEvaluator expressionEvaluator,
        IStringLocalizer<NotifyUserTask> localizer,
        IHttpContextAccessor httpContextAccessor,
        HtmlEncoder htmlEncoder,
        ILogger<NotifyUserTask> logger
    )
    {
        _notificationCoordinator = notificationCoordinator;
        _expressionEvaluator = expressionEvaluator;
        S = localizer;
        _httpContextAccessor = httpContextAccessor;
        _htmlEncoder = htmlEncoder;
        _logger = logger;
    }

    public override string Name => nameof(NotifyUserTask);

    public override LocalizedString DisplayText => S["Notify User Task"];

    public override LocalizedString Category => S["Notifications"];

    public WorkflowExpression<string> Subject
    {
        get => GetProperty(() => new WorkflowExpression<string>());
        set => SetProperty(value);
    }

    public WorkflowExpression<string> Body
    {
        get => GetProperty(() => new WorkflowExpression<string>());
        set => SetProperty(value);
    }

    public WorkflowExpression<bool> IsHtmlBody
    {
        get => GetProperty(() => new WorkflowExpression<bool>());
        set => SetProperty(value);
    }

    public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        return Outcomes(S["Done"], S["Failed"], S["Failed - User not found"]);
    }

    public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        if (!workflowContext.Input.TryGetValue("User", out var userObject) || userObject is not IUser su)
        {
            return Outcomes("Failed - User not found");
        }

        var subject = await _expressionEvaluator.EvaluateAsync(Subject, workflowContext, _htmlEncoder);
        var body = await _expressionEvaluator.EvaluateAsync(Body, workflowContext, _htmlEncoder);
        var isHtmlBody = await _expressionEvaluator.EvaluateAsync(IsHtmlBody, workflowContext, _htmlEncoder);

        var message = GetMessage(subject, body, isHtmlBody);

        var result = await _notificationCoordinator.TrySendAsync(su, message);
        workflowContext.LastResult = result;

        if (result == 0)
        {
            return Outcomes("Failed");
        }

        return Outcomes("Done");
    }

    private static NotificationMessage GetMessage(string subject, string body, bool isHtmlBody)
    {
        var message = new NotificationMessage()
        {
            Subject = subject,
        };

        if (isHtmlBody)
        {
            message.HtmlBody = body;
        }
        else
        {
            message.TextBody = body;
        }

        return message;
    }
}
