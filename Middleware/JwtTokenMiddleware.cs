using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace sjam.Middleware
{
    public class JwtTokenMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<JwtTokenMiddleware> _logger;
        private readonly IConfiguration _Configuration;

        public JwtTokenMiddleware(RequestDelegate next, ILogger<JwtTokenMiddleware> logger, IConfiguration configuration)
        {
            _next = next;
            _logger = logger;
            _Configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                if (token != null)
                {
                    var jwtSettings = context.RequestServices.GetRequiredService<IConfiguration>().GetSection("Auth");
                    var signingKey = jwtSettings["SecretKey"];
                    if (string.IsNullOrEmpty(signingKey))
                    {
                        throw new Exception("JWT Signing Key is not configured in appsettings.");
                    }

                    var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                    var validationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = bool.Parse(jwtSettings["ValidateIssuer"] ?? "false"),
                        ValidateAudience = bool.Parse(jwtSettings["ValidateAudience"] ?? "false"),
                        ValidateLifetime = bool.Parse(jwtSettings["ValidateLifetime"] ?? "false"),
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings["Issuer"],
                        ValidAudience = jwtSettings["Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
                    };

                    // Validate the token
                    tokenHandler.ValidateToken(token, validationParameters, out _);

                    // If valid, set the token in the Authorization header
                    context.Request.Headers["Authorization"] = "Bearer " + token;

                    await _next(context);
                }
                else
                {
                    //not required to authenticate
                    await _next(context);
                    //throw new Exception("ErrorMessages.Invalid_token");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                if (ex.Message == "ErrorMessages.Invalid_token")
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "text/plain";
                    await context.Response.WriteAsync("Invalid token!");
                }
                else
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "text/plain";
                    await context.Response.WriteAsync("Unauthorized access!");
                }
            }
        }



    }

    public static class JwtTokenMiddlewareExtensions
    {
        public static IApplicationBuilder UseJwtTokenMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<JwtTokenMiddleware>();
        }
    }
}
