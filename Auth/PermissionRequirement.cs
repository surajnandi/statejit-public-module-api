using Microsoft.AspNetCore.Authorization;

namespace sjam.Auth
{
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public string[] Permissions { get; }
        public PermissionRequirement(params string[] permissions)
        {
            Permissions = permissions;
        }
    }

}
