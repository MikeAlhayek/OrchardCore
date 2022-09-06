using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentFields.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Notifications.Models;
using OrchardCore.Notifications.ViewModels;

namespace OrchardCore.Notifications.Drivers;

public class NotificationReceiverPartDisplayDriver : ContentPartDisplayDriver<NotificationReceiverPart>
{
    private readonly IStringLocalizer S;

    public NotificationReceiverPartDisplayDriver(IStringLocalizer<NotificationReceiverPartDisplayDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(NotificationReceiverPart part)
    {
        return Initialize<NotificationReceiverPartViewModel>("NotificationReceiverPart_Edit", model =>
        {
            model.Receivers = part.Receivers ?? Array.Empty<string>();
            model.Options = GetReceiverOptions();
        });
    }

    public override async Task<IDisplayResult> UpdateAsync(NotificationReceiverPart part, IUpdateModel updater, UpdatePartEditorContext context)
    {
        var model = new NotificationReceiverPartViewModel();

        if (await updater.TryUpdateModelAsync(model, Prefix))
        {
            if (model.Receivers.Contains(NotificationTemplateConstants.SpecificUsersValue, StringComparer.OrdinalIgnoreCase))
            {
                var usersModel = new EditUserPickerFieldViewModel();

                await updater.TryUpdateModelAsync(usersModel, NotificationTemplateConstants.NotificationReceivingUsersPart + ".Users");

                if (usersModel.UserIds == null || usersModel.UserIds.Length == 0)
                {
                    updater.ModelState.AddModelError(Prefix, nameof(usersModel.UserIds), S["User is required."]);
                }
            }
        }

        part.Receivers = model.Receivers;

        return Edit(part);
    }

    private static IEnumerable<NotificationReceiverOption> GetReceiverOptions()
    {
        yield return new NotificationReceiverOption
        {
            Text = "Current user",
            Value = NotificationTemplateConstants.CurrentUserValue
        };

        yield return new NotificationReceiverOption
        {
            Text = "Specific user",
            Value = NotificationTemplateConstants.SpecificUsersValue
        };

        yield return new NotificationReceiverOption
        {
            Text = "Content item's owner",
            Value = NotificationTemplateConstants.ContentItemOwnerValue,
            IsContentBased = true,
        };
    }
}
