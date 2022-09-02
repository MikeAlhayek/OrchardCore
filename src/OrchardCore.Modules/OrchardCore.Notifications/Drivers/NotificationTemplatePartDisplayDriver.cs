using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Notifications.Models;
using OrchardCore.Notifications.ViewModels;

namespace OrchardCore.Notifications.Drivers;

public class NotificationTemplatePartDisplayDriver : ContentPartDisplayDriver<NotificationTemplatePart>
{
    private readonly IEnumerable<INotificationTemplateProvider> _notificationTemplateProviders;
    private readonly IStringLocalizer S;

    public NotificationTemplatePartDisplayDriver(IEnumerable<INotificationTemplateProvider> notificationTemplateProviders,
        IStringLocalizer<NotificationTemplatePartDisplayDriver> stringLocalizer)
    {
        _notificationTemplateProviders = notificationTemplateProviders;
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(NotificationTemplatePart part, BuildPartEditorContext context)
    {
        return Initialize<NotificationTemplatePartViewModel>(GetEditorShapeType(context), model =>
        {
            model.TemaplateName = part.TemplateName;

            var options = new List<TemplateOption>();

            foreach (var provider in _notificationTemplateProviders)
            {
                options.Add(new TemplateOption
                {
                    Name = provider.Name,
                    Description = provider.Description,
                    Arguments = provider.GetArguments()
                });
            }
            model.Options = options.OrderBy(x => x.Description);
        });
    }

    public override async Task<IDisplayResult> UpdateAsync(NotificationTemplatePart part, IUpdateModel updater, UpdatePartEditorContext context)
    {
        var model = new NotificationTemplatePartViewModel();

        if (await context.Updater.TryUpdateModelAsync(model, Prefix))
        {
            var template = _notificationTemplateProviders.FirstOrDefault(x => String.Equals(x.Name, model.TemaplateName));

            if (template is null)
            {
                updater.ModelState.AddModelError(Prefix, nameof(NotificationTemplatePartViewModel.TemaplateName), S["Invalid template."]);
            }

            part.TemplateName = template?.Name;
        }

        return Edit(part, context);
    }
}
