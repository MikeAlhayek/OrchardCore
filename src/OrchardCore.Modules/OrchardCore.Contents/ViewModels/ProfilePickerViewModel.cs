using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.Contents.ViewModels;

public class ProfilePickerViewModel
{
    [Required]
    public string ContentItemId { get; set; }

    [BindNever]
    public string ContentType { get; set; }

    [BindNever]
    public IList<VueMultiselectItemViewModel> SelectedItems { get; set; }
}
