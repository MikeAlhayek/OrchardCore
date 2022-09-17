using OrchardCore.Security.Permissions;

namespace OrchardCore.Infrastructure.Security.Permissions;

public interface IPermissionLocalizer
{
    /// <summary>
    /// Returns a permission with a localized description
    /// </summary>
    /// <param name="permission"></param>
    /// <returns></returns>
    Permission Localize(Permission permission);
}
