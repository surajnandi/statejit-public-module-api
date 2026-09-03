using Microsoft.AspNetCore.Authorization;
using System.Text.Json;

namespace sjam.Auth
{
    public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            var permissionsClaim = context.User.FindFirst(c => c.Type == "permissions");

            if (permissionsClaim != null)
            {
                try
                {
                    var userPermissions = JsonSerializer.Deserialize<List<string>>(permissionsClaim.Value);

                    if (userPermissions != null)
                    {
                        if (userPermissions.Any(requirement.Permissions.Contains))
                        {
                            context.Succeed(requirement);
                        }
                    }
                }
                catch (JsonException)
                {
                }
            }

            return Task.CompletedTask;


        }
    }

}
