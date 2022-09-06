using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Notifications.Models;

namespace OrchardCore.Notifications.ViewModels;

public class NotificationReceiverPartViewModel
{
    public string[] Receivers { get; set; }

    [BindNever]
    public IEnumerable<NotificationReceiverOption> Options { get; set; }
}
