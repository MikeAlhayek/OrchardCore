using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.Notifications.ViewModels;

public class ContentNotificationSettingsViewModel
{
    public bool SendNotification { get; set; }

    public string EventName { get; set; }

    public string[] NotificationTemplateContentItemIds { get; set; }

    [BindNever]
    public IEnumerable<SelectListItem> EventNames { get; set; }

    [BindNever]
    public IEnumerable<SelectListItem> NotificationTemplates { get; set; }
}
