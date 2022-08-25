using System.ComponentModel;
using OrchardCore.ContentManagement;

namespace OrchardCore.Forms.Models
{
    public class InputPart : ContentPart
    {
        public string Type { get; set; }

        public string DefaultValue { get; set; }

        public string Placeholder { get; set; }

        public LabelOptions LabelOption { get; set; }

        public string Label { get; set; }

        public ValidationOptions ValidationOption { get; set; }
    }
}
