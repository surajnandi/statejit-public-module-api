using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace sjam.Auth
{
    public static class JwtAuthentication
    {
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services)
        {
            // Access configuration directly from the IServiceCollection
            var configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();

            var jwtSettings = configuration.GetSection("Auth");
            var signingKey = jwtSettings["SecretKey"];

            if (string.IsNullOrEmpty(signingKey))
            {
                throw new ArgumentException("JWT Signing Key is not configured in appsettings.");
            }

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = bool.Parse(jwtSettings["ValidateIssuer"] ?? "false"),
                    ValidateAudience = bool.Parse(jwtSettings["ValidateAudience"] ?? "false"),
                    ValidateLifetime = bool.Parse(jwtSettings["ValidateLifetime"] ?? "false"),
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;

                        if (!string.IsNullOrEmpty(accessToken) &&
                            (path.StartsWithSegments("/notification")))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };


            });

            return services;
        }
    }

}
