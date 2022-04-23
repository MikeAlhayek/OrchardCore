using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Environment.Shell.Models
{
    public class FeatureProfile
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public List<FeatureRule> FeatureRules = new List<FeatureRule>();

        // should we inherit from Entity instead?
        // public JObject Properties { get; set; } = new JObject();

    }

    public class FeatureRule
    {
        [Required]
        public string Rule { get; set; }

        [Required]
        public string Expression { get; set; }
    }
}
