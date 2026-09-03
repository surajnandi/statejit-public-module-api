using Microsoft.AspNetCore.Authorization;

namespace sjam.Auth
{
    public static class AuthorizationService
    {
        public static void AddAuthorizationPolicies(this IServiceCollection services)
        {
            // Register custom authorization handler
            services.AddSingleton<IAuthorizationHandler, PermissionHandler>();

            // Add authorization policies
            services.AddAuthorization(options =>
            {
                #region Roles Policy
                options.AddPolicy("all-permission", policy =>
                    policy.Requirements.Add(new PermissionRequirement("all-permission")));

                options.AddPolicy("admin", policy =>
                    policy.RequireRole("admin"));

                options.AddPolicy("approver", policy =>
                    policy.RequireRole("approver"));

                options.AddPolicy("operator", policy =>
                    policy.RequireRole("operator"));

                // Do Not Allow Permission
                options.AddPolicy("DoNotAllow", policy => policy.RequireAssertion(_ => false));
                #endregion

                #region Roles Permissions
                options.AddPolicy("can-view-fto", policy =>
                {
                    policy.RequireRole("approver", "operator", "admin");
                    policy.Requirements.Add(new PermissionRequirement("can-view-fto"));
                });

                options.AddPolicy("can-reject-fto", policy =>
                {
                    policy.RequireRole("approver", "operator");
                    policy.Requirements.Add(new PermissionRequirement("can-reject-fto"));
                });

                options.AddPolicy("can-approve-agency-ddo-map", policy =>
                {
                    policy.RequireRole("approver");
                    policy.Requirements.Add(new PermissionRequirement("can-approve-agency-ddo-map"));
                });

                options.AddPolicy("can-view-hoa-detail", policy =>
                {
                    policy.RequireRole("approver", "admin");
                    policy.Requirements.Add(new PermissionRequirement("can-view-hoa-detail"));
                });

                options.AddPolicy("can-view-scheme-detail", policy =>
                {
                    policy.RequireRole("approver", "admin");
                    policy.Requirements.Add(new PermissionRequirement("can-view-scheme-detail"));
                });

                options.AddPolicy("can-check-allotment", policy =>
                {
                    policy.RequireRole("approver", "operator", "admin");
                    policy.Requirements.Add(new PermissionRequirement("can-check-allotment"));
                });

                options.AddPolicy("can-forward-bill-approver", policy =>
                {
                    policy.RequireRole("operator");
                    policy.Requirements.Add(new PermissionRequirement("can-forward-bill-approver"));
                });

                options.AddPolicy("can-forward-bill-treasury", policy =>
                {
                    policy.RequireRole("approver");
                    policy.Requirements.Add(new PermissionRequirement("can-forward-bill-treasury"));
                });

                options.AddPolicy("can-regenerate-cpin-failed", policy =>
                {
                    policy.RequireRole("approver");
                    policy.Requirements.Add(new PermissionRequirement("can-regenerate-cpin-failed"));
                });
                options.AddPolicy("can-view-report", policy =>
                {
                    policy.RequireRole("approver", "operator", "admin");
                    policy.Requirements.Add(new PermissionRequirement("can-view-report"));
                });
                options.AddPolicy("can-view-master-records", policy =>
                {
                    policy.RequireRole("approver");
                    policy.Requirements.Add(new PermissionRequirement("can-view-master-records"));
                });
                options.AddPolicy("can-view-jit-report", policy =>
                {
                    policy.RequireRole("approver");
                    policy.Requirements.Add(new PermissionRequirement("can-view-jit-report"));
                });
                options.AddPolicy("can-create-bill", policy =>
                {
                    policy.RequireRole("approver", "operator");
                    policy.Requirements.Add(new PermissionRequirement("can-create-bill"));
                });
                options.AddPolicy("can-view-jit-bill", policy =>
                {
                    policy.RequireRole("approver", "operator");
                    policy.Requirements.Add(new PermissionRequirement("can-view-jit-bill"));
                });
                options.AddPolicy("can-view-successful-benf-list", policy =>
                {
                    policy.RequireRole("admin", "approver");
                    policy.Requirements.Add(new PermissionRequirement("can-view-successful-benf-list"));
                });
                options.AddPolicy("can-view-failed-benf-list", policy =>
                {
                    policy.RequireRole("admin", "approver");
                    policy.Requirements.Add(new PermissionRequirement("can-view-failed-benf-list"));
                });
                options.AddPolicy("can-view-bill-wise-report", policy =>
                {
                    policy.RequireRole("admin", "approver", "operator");
                    policy.Requirements.Add(new PermissionRequirement("can-view-bill-wise-report"));
                });
                options.AddPolicy("can-view-pfms-report", policy =>
                {
                    policy.RequireRole("admin", "approver", "operator");
                    policy.Requirements.Add(new PermissionRequirement("can-view-pfms-report"));
                });
                options.AddPolicy("can-view-cpin", policy =>
                {
                    policy.RequireRole("approver");
                    policy.Requirements.Add(new PermissionRequirement("can-view-cpin"));
                });
                options.AddPolicy("can-insert-cpin", policy =>
                {
                    policy.RequireRole("approver");
                    policy.Requirements.Add(new PermissionRequirement("can-insert-cpin"));
                });
                options.AddPolicy("can-update-ecs-cancellation", policy =>
                {
                    policy.RequireRole("approver");
                    policy.Requirements.Add(new PermissionRequirement("can-update-ecs-cancellation"));
                });

                options.AddPolicy("can-create-css-refund-bill", policy =>
                {
                    policy.RequireRole("approver", "operator");
                    policy.Requirements.Add(new PermissionRequirement("can-create-css-refund-bill"));
                });

                // Admin Permission
                options.AddPolicy("can-view-failed-transaction", policy =>
                {
                    policy.RequireRole("admin");
                    policy.Requirements.Add(new PermissionRequirement("can-view-failed-transaction"));
                });
                options.AddPolicy("can-view-transaction-summary", policy =>
                {
                    policy.RequireRole("admin");
                    policy.Requirements.Add(new PermissionRequirement("can-view-transaction-summary"));
                });
                options.AddPolicy("can-send-legacy-data", policy =>
                {
                    policy.RequireRole("admin");
                    policy.Requirements.Add(new PermissionRequirement("can-send-legacy-data"));
                });
                options.AddPolicy("can-view-rabbitmq-transaction", policy =>
                {
                    policy.RequireRole("admin");
                    policy.Requirements.Add(new PermissionRequirement("can-view-rabbitmq-transaction"));
                });

                #endregion
            });
        }
    }

}
