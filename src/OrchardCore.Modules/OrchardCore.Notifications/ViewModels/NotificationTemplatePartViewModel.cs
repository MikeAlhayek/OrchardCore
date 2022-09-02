using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContentFields.Fields;
using OrchardCore.Notifications.Models;

namespace OrchardCore.Notifications.ViewModels;

public class NotificationTemplatePartViewModel
{
    [Required]
    public string TemaplateName { get; set; }

    [Required]
    public TextField SendTo { get; set; }

    public UserPickerField Users { get; set; }

    [BindNever]
    public IEnumerable<TemplateOption> Options { get; set; }
}
