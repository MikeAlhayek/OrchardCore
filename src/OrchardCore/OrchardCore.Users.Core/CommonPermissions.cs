using System;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Users
{
    public class CommonPermissions
    {
        /// <summary>
        /// When authorizing request ManageUsers and pass an <see cref="IUser"/>
        /// Do not request a dynamic permission unless you are checking if the user can manage a specific role.
        /// </summary>
        public static readonly Permission ManageUsers = new("ManageUsers", "Manage security settings and all users", true);

        /// <summary>
        /// View users only allows viewing a users profile.
        /// </summary>
        public static readonly Permission ViewUsers = new("View Users", "View user's profile", new[] { ManageUsers });

        public static readonly Permission EditUsers = new("EditUsers", "Edit any user", new[] { ManageUsers }, true);

        public static readonly Permission DeleteUsers = new("DeleteUsers", "Delete any user", new[] { ManageUsers }, true);

        public static readonly Permission ListUsers = new("ListUsers", "List all users", new[] { EditUsers, DeleteUsers });

        public static readonly Permission AssignUsersToRole = new("AssignUsersToRole", "Assign users to any role", new[] { EditUsers }, true);

        public static Permission CreateListUsersInRolePermission(string name) =>
            CreateDynamicPermission(name, new Permission("ListUsersInRole_{0}", "List users in {0} role", new[] { ListUsers }));

        public static Permission CreateEditUsersInRolePermission(string name) =>
            CreateDynamicPermission(name, new Permission("EditUsersInRole_{0}", "Edit users in {0} role", new[] { EditUsers }, true));

        public static Permission CreateDeleteUsersInRolePermission(string name) =>
            CreateDynamicPermission(name, new Permission("DeleteUsersInRole_{0}", "Delete users in {0} role", new[] { DeleteUsers }, true));

        public static Permission CreateAssignUsersToRolePermission(string name) =>
            CreateDynamicPermission(name, new Permission("AssignUsersToRole_{0}", "Assign users to {0} role", new[] { AssignUsersToRole }, true));

        public static Permission CreatePermissionForManageUsersInRole(string name) =>
            CreateDynamicPermission(name, new Permission("ManageUsersInRole_{0}", "Manage users in {0} role", new[] { ManageUsers }, true));

        // Dynamic permission template.
        private static Permission CreateDynamicPermission(string name, Permission permission)
            => new(
                String.Format(permission.Name, name),
                String.Format(permission.Description, name),
                permission.ImpliedBy,
                permission.IsSecurityCritical
            );
    }
}
