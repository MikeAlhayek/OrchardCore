using OrchardCore.Security.Permissions;

namespace OrchardCore.Cms.OnDemandFeatures;

public class NotifyPermissions
{
    public static readonly Permission Notify = new Permission(nameof(Notify), "Send user notifications manually");

}
