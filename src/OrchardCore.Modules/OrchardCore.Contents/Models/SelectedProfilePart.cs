using System.ComponentModel.DataAnnotations;
using OrchardCore.ContentManagement;

namespace OrchardCore.Contents.Models;

public class SelectedProfilePart : ContentPart
{
    [Required]
    public string ProfileContentItemId { get; set; }
}
