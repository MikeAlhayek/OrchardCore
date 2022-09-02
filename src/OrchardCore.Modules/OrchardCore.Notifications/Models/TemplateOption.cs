using System.Collections.Generic;

namespace OrchardCore.Notifications.Models;

public class TemplateOption
{
    public string Name { get; set; }
    public string Description { get; set; }
    public IEnumerable<string> Arguments { get; set; }
}
