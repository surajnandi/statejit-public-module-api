using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using sjam.Bal.Interfaces;
using sjam.Dal.Enum;

namespace sjam.Helpers
{
    public sealed class ApiConfigFilter : IAsyncActionFilter
    {
        private readonly IAuthClaimService _authClaimService;

        public ApiConfigFilter(IAuthClaimService authClaimService)
        {
            _authClaimService = authClaimService;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var role = _authClaimService.User.Role;

            if (string.Equals(role, RoleEnum.ADMIN, StringComparison.OrdinalIgnoreCase))
            {
                await next();
                return;
            }

            var controller = context.RouteData.Values["controller"]?.ToString();

            var action =
                context.HttpContext.Request.Path.Value?
                    .Split('/')
                    .LastOrDefault();

            if (string.IsNullOrWhiteSpace(controller) || string.IsNullOrWhiteSpace(action))
            {
                await next();
                return;
            }

            if (ApiConfigHelper.TryGet(controller, action, out var config) && config != null)
            {
                bool enabled = config.IsActive.GetValueOrDefault();

                var now = DateTime.Now;

                if (config.ScheduledStart.HasValue &&
                    config.ScheduledEnd.HasValue)
                {
                    if (now >= config.ScheduledStart.Value &&
                        now <= config.ScheduledEnd.Value)
                    {
                        enabled = false;
                    }
                }
                else if (config.ScheduledStart.HasValue)
                {
                    if (now >= config.ScheduledStart.Value)
                    {
                        enabled = false;
                    }
                }
                else if (config.ScheduledEnd.HasValue)
                {
                    if (now <= config.ScheduledEnd.Value)
                    {
                        enabled = false;
                    }
                }

                if (!enabled)
                {
                    context.Result =
                        new ObjectResult(new
                        {
                            success = false,
                            message = config.Message
                        })
                        {
                            StatusCode = StatusCodes.Status503ServiceUnavailable
                        };

                    return;
                }
            }

            await next();
        }
    }
}
