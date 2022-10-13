using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Notifications.ViewModels;

public class NotifyUserTaskTaskViewModel
{
    [Required]
    public string Subject { get; set; }

    public string Body { get; set; }

    public bool IsHtmlBody { get; set; }
}
