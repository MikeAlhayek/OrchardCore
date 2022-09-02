using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;

namespace OrchardCore.Notifications.Models;

public class NotificationTemplateDeliveryPart : ContentPart, IValidatableObject
{
    public TextField SendTo { get; set; }

    public UserPickerField Users { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var S = validationContext.GetService<IStringLocalizer<NotificationTemplateDeliveryPart>>();

        if (SendTo == null || String.IsNullOrEmpty(SendTo.Text))
        {
            yield return new ValidationResult(S["Visible To is reaquired field"]);
        }

        if (NotificationTemplateConstants.SpecificUsersValue.Equals(SendTo?.Text) && (Users == null || Users.UserIds == null || !Users.UserIds.Where(x => !String.IsNullOrWhiteSpace(x)).Any()))
        {
            yield return new ValidationResult(S["At least one user must be selected"]);
        }
    }
}
