using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Cms.OnDemandFeatures;

public class NotifyPermissionProvider : IPermissionProvider
{
    public IEnumerable<PermissionStereotype> GetDefaultStereotypes() =>
        new[]
        {
                new PermissionStereotype
                {
                    Name = "Administrator",
                    Permissions = new[]
                    {
                        NotifyPermissions.Notify,
                    },
                },
        };

    public Task<IEnumerable<Permission>> GetPermissionsAsync() =>
        Task.FromResult(new[]
        {
                NotifyPermissions.Notify,
        }
        .AsEnumerable());
}
