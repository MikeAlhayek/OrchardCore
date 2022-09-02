using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Notifications.Abstractions;
using OrchardCore.Notifications.Core;
using OrchardCore.Notifications.ViewModels;
using OrchardCore.Users.Models;

namespace OrchardCore.Notifications.Drivers;

public class UserNotificationPartDisplayDriver : SectionDisplayDriver<User, UserNotificationPart>
{
    private readonly IEnumerable<INotificationSender> _notificationSenders;
    private readonly IStringLocalizer S;

    public UserNotificationPartDisplayDriver(IEnumerable<INotificationSender> notificationSenders,
        IStringLocalizer<UserNotificationPartDisplayDriver> stringLocalizer)
    {
        _notificationSenders = notificationSenders;
        S = stringLocalizer;
    }

    public override Task<IDisplayResult> EditAsync(User user, UserNotificationPart part, BuildEditorContext context)
    {
        var result = Initialize<UserNotificationViewModel>("UserNotificationPart_Edit", viewModel =>
        {
            var selectedTypes = new List<string>(part.Types ?? Array.Empty<string>());
            viewModel.Types = selectedTypes.ToArray();

            viewModel.AvailableTypes = _notificationSenders
                .Select(sender => new SelectListItem(S[sender.Name], sender.Type))
                // sort the types in the same order they are saved to honor the priority order
                .OrderBy(x => selectedTypes.IndexOf(x.Value));

        }).Location("Content:11");

        return Task.FromResult<IDisplayResult>(result);
    }

    public override async Task<IDisplayResult> UpdateAsync(User user, UserNotificationPart part, IUpdateModel updater, BuildEditorContext context)
    {
        var vm = new UserNotificationViewModel();

        if (await updater.TryUpdateModelAsync(vm, Prefix))
        {
            part.Types = vm.Types ?? Array.Empty<string>();
        }

        return await EditAsync(user, part, context);
    }
}
