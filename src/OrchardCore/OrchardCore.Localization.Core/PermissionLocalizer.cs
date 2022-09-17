using System;
using Microsoft.Extensions.Localization;
using OrchardCore.Infrastructure.Security.Permissions;
using OrchardCore.Security.Permissions;

public class PermissionLocalizer : IPermissionLocalizer
{
    private readonly IStringLocalizer<PermissionLocalizer> S;

    public PermissionLocalizer(IStringLocalizer<PermissionLocalizer> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public Permission Localize(Permission permission)
    {
        if (String.IsNullOrEmpty(permission.Description))
        {
            return permission;
        }

        return new Permission(permission.Name, S[permission.Description], permission.ImpliedBy, permission.IsSecurityCritical);
    }
}
