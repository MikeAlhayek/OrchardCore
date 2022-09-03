using System.Collections.Generic;

namespace OrchardCore.Notifications.Models;

public class TemplateOption
{
    public string Id { get; set; }
    public string Description { get; set; }
    public IEnumerable<string> Arguments { get; set; }
    public string Title { get; internal set; }
}
