using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Notifications.Models;
using OrchardCore.Notifications.ViewModels;
using OrchardCore.Users.Models;

namespace OrchardCore.Notifications.Drivers;

public class UserNotificationPartDisplayDriver : SectionDisplayDriver<User, UserNotificationPart>
{
    private readonly IEnumerable<INotificationMethodProvider> _notificationMethodProviders;

    public UserNotificationPartDisplayDriver(IEnumerable<INotificationMethodProvider> notificationMethodProviders)
    {
        _notificationMethodProviders = notificationMethodProviders;
    }

    public override Task<IDisplayResult> EditAsync(User user, UserNotificationPart part, BuildEditorContext context)
    {
        var result = Initialize<UserNotificationViewModel>("UserNotificationPart_Edit", model =>
        {
            var sortedMethods = new List<string>(part.Methods ?? Array.Empty<string>());
            var optout = part.Optout ?? Array.Empty<string>();

            // By default the use is opted into all available methods until explicitly optout.
            model.Methods = _notificationMethodProviders.Select(x => x.Method).Except(optout, StringComparer.OrdinalIgnoreCase).ToArray();

            model.Optout = optout;
            model.Strategy = part.Strategy;

            model.Strategies = new List<SelectListItem>()
            {
                new SelectListItem("Notify all methods", UserNotificationStrategy.AllMethods.ToString()),
                new SelectListItem("Notify until the first success", UserNotificationStrategy.UntilFirstSuccess.ToString()),
            };

            var availableItems = _notificationMethodProviders
                .Select(provider => new SelectListItem(provider.Name, provider.Method));

            if (sortedMethods.Count > 0)
            {
                model.AvailableMethods = availableItems
                // Sort the methods in the same order they are saved to honor the priority order (i.e., user preferences.)
                .OrderBy(x => sortedMethods.IndexOf(x.Value))
                .ThenBy(x => x.Text);
            }
            else
            {
                model.AvailableMethods = availableItems.OrderBy(x => x.Text);
            }

        }).Location("Content:11");

        return Task.FromResult<IDisplayResult>(result);
    }

    public override async Task<IDisplayResult> UpdateAsync(User user, UserNotificationPart part, IUpdateModel updater, BuildEditorContext context)
    {
        var model = new UserNotificationViewModel();

        if (await updater.TryUpdateModelAsync(model, Prefix))
        {
            var sortedMethods = new List<string>(model.SortedMethods ?? Array.Empty<string>());

            if (sortedMethods.Count > 0)
            {
                // Important to check execute this code only when selectedOrdrededMethods has at least one element to avoid exception.
                // Store all methods in the same order then appear.
                part.Methods = _notificationMethodProviders
                    .OrderBy(provider => sortedMethods.IndexOf(provider.Method))
                    .ThenBy(provider => provider.Name)
                    .Select(x => x.Method)
                    .ToArray();
            }
            else
            {
                part.Methods = _notificationMethodProviders.OrderBy(provider => provider.Name)
                    .Select(x => x.Method)
                    .ToArray();
            }

            var selectedMethods = new List<string>(model.Methods ?? Array.Empty<string>());

            // Store any method that is not selected as an optout.
            part.Optout = _notificationMethodProviders.Where(provider => !selectedMethods.Contains(provider.Method, StringComparer.OrdinalIgnoreCase))
                .Select(provider => provider.Method)
                .ToArray();

            part.Strategy = model.Strategy;
        }

        return await EditAsync(user, part, context);
    }
}
