using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using OrchardCore.Cms.OnDemandFeatures.Abstractions;
using OrchardCore.Cms.OnDemandFeatures.Core;
using OrchardCore.Cms.OnDemandFeatures.ViewModels;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Users.Models;

namespace OrchardCore.Cms.OnDemandFeatures.Drivers;


public class UserNotificationPartDisplayDriver : SectionDisplayDriver<User, UserNotificationPart>
{
    private readonly IEnumerable<INotificationSender> _notificationSenders;
    private readonly IStringLocalizer _s;

    public UserNotificationPartDisplayDriver(IEnumerable<INotificationSender> notificationSenders,
        IStringLocalizer<UserNotificationPartDisplayDriver> stringLocalizer)
    {
        _notificationSenders = notificationSenders;
        _s = stringLocalizer;
    }

    public override Task<IDisplayResult> EditAsync(User user, UserNotificationPart part, BuildEditorContext context)
    {
        var result = Initialize<UserNotificationViewModel>("UserNotificationPart_Edit", viewModel =>
        {
            var selectedTypes = new List<string>(part.Types ?? Array.Empty<string>());
            viewModel.Types = selectedTypes.ToArray();

            viewModel.AvailableTypes = _notificationSenders
                .Select(sender => new SelectListItem(_s[sender.Name], sender.Type))
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
