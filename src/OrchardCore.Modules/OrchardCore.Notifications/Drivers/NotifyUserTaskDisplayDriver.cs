using OrchardCore.Notifications.Activities;
using OrchardCore.Notifications.ViewModels;
using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Notifications.Drivers;

public class NotifyUserTaskDisplayDriver : ActivityDisplayDriver<NotifyUserTask, NotifyUserTaskTaskViewModel>
{
    protected override void EditActivity(NotifyUserTask activity, NotifyUserTaskTaskViewModel model)
    {
        model.Subject = activity.Subject.Expression;
        model.Body = activity.Body.Expression;
        model.IsHtmlBody = activity.IsHtmlBody.Expression.ToLowerInvariant() == "true";
    }

    protected override void UpdateActivity(NotifyUserTaskTaskViewModel model, NotifyUserTask activity)
    {
        activity.Subject = new WorkflowExpression<string>(model.Subject);
        activity.Body = new WorkflowExpression<string>(model.Body);
        activity.IsHtmlBody = new WorkflowExpression<bool>(model.IsHtmlBody.ToString());
    }
}
