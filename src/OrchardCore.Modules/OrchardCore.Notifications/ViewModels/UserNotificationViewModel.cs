using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.Notifications.ViewModels;

public class UserNotificationViewModel
{
    public string[] Types { get; set; }

    [BindNever]
    public IEnumerable<SelectListItem> AvailableTypes { get; set; }
}
