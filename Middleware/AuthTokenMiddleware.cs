using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.IdentityModel.Tokens;
using sjam.Dal.Enum;
using sjam.Helpers;
using sjam.Models;
using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace sjam.Middleware
{
    public class AuthTokenMiddleware
    (
        RequestDelegate next,
        ILogger<AuthTokenMiddleware> logger,
        IConfiguration Configuration
    )
    {
        private readonly RequestDelegate _next = next;
        private readonly IConfiguration _Configuration = Configuration;
        private readonly ILogger<AuthTokenMiddleware> _logger = logger;

        private static ConcurrentDictionary<string, string> _allowedTokens = [];
        private static ConcurrentDictionary<string, string> _userSessions = [];


        private void RemoveExpiredTokens()
        {
            JwtSecurityTokenHandler tokenHandler = new();
            bool removeStatus = false;
            string removedValue;

            _allowedTokens.Where(
                c => {
                    if (tokenHandler.CanReadToken(c.Value) == false)
                    {
                        return false;
                    }
                    return long.Parse(tokenHandler.ReadJwtToken(c.Value)
                        .Claims.FirstOrDefault(
                            c => c.Type == JwtRegisteredClaimNames.Exp
                        )?.Value ?? "0") <= (DateTimeOffset.UtcNow.ToUnixTimeSeconds() - 3600);
                })
                .Select(c => c.Key)
                .ToList().ForEach(
                    item =>
                    {
                        _allowedTokens.TryRemove(item, out removedValue);
                        _userSessions.TryRemove(item, out removedValue);

                        Console.WriteLine($"Removing Token: {item}");
                    }
                );
        }

        private static async Task Prepare401Response(string message, HttpContext context)
        {
            ServiceResponse<bool> response = new()
            {
                Message = message,
                ResponseStatus = APIResponseStatus.Error,
                Result = false
            };
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json; charset=utf-8";
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }

        public async Task Invoke(HttpContext context)
        {
            var endpoint = context.GetEndpoint();

            if (endpoint?.Metadata.GetMetadata<NoAuthorizeAttribute>() != null)
            {
                await _next(context);
                return;
            }

            RemoveExpiredTokens();

            //// Detect SignalR WebSocket handshake
            var isSignalR = context.Request.Path.HasValue && context.Request.Path.StartsWithSegments("/notification", StringComparison.OrdinalIgnoreCase);
            var isWebSocket = context.WebSockets.IsWebSocketRequest;


            var response = new ServiceResponse<bool>();

            string tokenId = "--";
            try
            {
                string token = context.Request.Headers.Authorization.FirstOrDefault()?.Split(" ").Last() ?? "";

                string aesKey = _Configuration?.GetSection("Auth:AesKey")?.Value ?? "";
                string aesIV = _Configuration?.GetSection("Auth:AesIV")?.Value ?? "";
                string secretKey = _Configuration?["Auth:SecretKey"] ?? "";
                string issuer = _Configuration?["Auth:Issuer"] ?? "";
                string audience = _Configuration?["Auth:Audience"] ?? "";

                bool validateIssuer = bool.Parse(_Configuration?["Auth:ValidateIssuer"] ?? "false");
                bool validateAudience = bool.Parse(_Configuration?["Auth:ValidateAudience"] ?? "false");
                bool validateLifetime = bool.Parse(_Configuration?["Auth:ValidateLifetime"] ?? "false");
                bool removeStatus = false;
                string removedValue;

                if (aesIV == "" || aesKey == "" || secretKey == "" || issuer == "" || audience == "")
                {
                    response.ResponseStatus = APIResponseStatus.Error;
                    response.Message = "Missing " + aesIV == "" ? "Auth:AesIV " : ""
                        + aesKey == "" ? "Auth:AesKey " : ""
                        + secretKey == "" ? "Auth:SecretKey " : ""
                        + issuer == "" ? "Auth:Issuer " : ""
                        + audience == "" ? "Auth:Audience " : "".Replace(" ", ", ").Trim([',', ' '])
                        + " in configuration, check appsettings. " + Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") + ".json";
                    response.Result = false;
                    return;
                }

                if (token != "")
                {

                    TokenValidationParameters? validationParameters = new()
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                        ValidateIssuer = validateIssuer,
                        ValidIssuer = issuer,
                        ValidateAudience = validateAudience,
                        ValidAudience = audience,
                        ValidateLifetime = validateLifetime, //false,
#if DEBUG
                        ClockSkew = TimeSpan.Zero
#endif

                    };

                    JwtSecurityTokenHandler tokenHandler = new();

                    ClaimsPrincipal? principal = tokenHandler.ValidateToken(
                        token,
                        validationParameters,
                        out SecurityToken validatedToken
                    );
                    var authClaims = principal.Claims.ToList();

                    bool isAuthenticated = false;

                    if (principal.Identity != null)
                    {
                        isAuthenticated = principal.Identity.IsAuthenticated;
                    }

                    AuthClaimModel? jwtAuthClaimModel = new()
                    {
                        claims = [.. principal.Claims]
                    };

                    if (isAuthenticated && jwtAuthClaimModel != null)
                    {

                        tokenId = principal.Claims.FirstOrDefault(
                                c => c.Type == JwtRegisteredClaimNames.Jti
                            )?.Value ?? "0";


                        if (tokenId == "0")
                        {
                            await Prepare401Response(
                                $"TokenID: {tokenId} is not valid.",
                                context
                            );
                            return;
                        }

                        string prevTokenId = principal.Claims.FirstOrDefault(
                                c => c.Type == "pti"
                            )?.Value ?? "0";

                        if (prevTokenId == "0")
                        {
                            await Prepare401Response(
                                $"PreviousTokenID: {tokenId} is not valid.",
                                context
                            );
                            return;
                        }

                        string tokenType = principal.Claims.FirstOrDefault(
                                c => c.Type == JwtRegisteredClaimNames.Typ
                            )?.Value ?? "0";

                        if (tokenType == "ref")
                        {
                            await Prepare401Response(
                                $"Use of refresh token ID: {tokenId} is not allowed to access APIs.",
                                context
                            );
                            return;
                        }


                        var headers = context.Request.Headers;

                        var tabId = headers.TryGetValue("x-tab-id", out var tid) ? tid.ToString() : "unknown";
                        var tabInstance = headers.TryGetValue("x-tab-instance-id", out var iid) ? iid.ToString() : "unknown";
                        var deviceId = headers.TryGetValue("x-device-id", out var did) ? did.ToString() : "unknown";
                        var isOverride = headers.TryGetValue("x-tab-override", out var over) && over == "true";

                        var userId = authClaims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? "0";
                        var connectionId = context.Connection.Id;

                        //string tokenUrlModel = _Configuration?["Auth:TokenUrlMode"] ?? "production";

                        var environment = _Configuration?.GetValue<AppEnvironment>("AppConfig:Environment") ?? AppEnvironment.PROD;

                        var isLocal = environment == AppEnvironment.DEV;

                        if (!isLocal)
                        {
                            if (!isSignalR && !isWebSocket)
                            {
                                if (deviceId == "unknown" || tabId == "unknown")
                                {
                                    await Prepare401Response("Missing required headers.", context);
                                    return;
                                }
                            }
                        }

                        var connectionKey = $"{connectionId}||{tabId}||{tabInstance}||{deviceId}";


                        if (_userSessions.TryGetValue(tokenId, out var savedConnectionKey))
                        {
                            var parts = savedConnectionKey.Split("||");
                            var savedConnectionId = parts.ElementAtOrDefault(0);
                            var savedTabId = parts.ElementAtOrDefault(1);
                            var savedTabInstance = parts.ElementAtOrDefault(2);
                            var savedDeviceId = parts.ElementAtOrDefault(3);

                            if (!isLocal)
                            {
                                if (!isSignalR && !isWebSocket)
                                {
                                    if (savedDeviceId != deviceId)
                                    {
                                        await Prepare401Response("ALREADY_LOGGED_IN_DIFFERENT_DEVICE", context);
                                        return;
                                    }

                                    if (savedTabId != tabId)
                                    {
                                        await Prepare401Response("ALREADY_LOGGED_IN_DIFFERENT_TAB", context);
                                        return;
                                    }

                                    if (savedTabInstance != tabInstance && !isOverride)
                                    {
                                        await Prepare401Response("DUPLICATE_TAB_OPEN", context);
                                        return;
                                    }

                                    if (savedTabInstance != tabInstance && isOverride)
                                    {
                                        _userSessions[tokenId] = connectionKey;
                                    }
                                }
                            }

                        }
                        else
                        {
                            _userSessions[tokenId] = connectionKey;
                        }



                        var tokenAlreadyAdded = _allowedTokens.ContainsKey(tokenId);

                        //string tokenUrlMode = _Configuration?["Auth:TokenUrlMode"] ?? "production";

                        if ((environment == AppEnvironment.DEV && !tokenAlreadyAdded) ||
                            (environment == AppEnvironment.PROD && context.Request.GetDisplayUrl().Contains("Login") && !tokenAlreadyAdded))
                        {
                            _allowedTokens.TryAdd(tokenId, token);
                            Console.WriteLine($"Using new token: {prevTokenId}:{tokenId}");
                        }

                        if (_allowedTokens.ContainsKey(prevTokenId))
                        {
                            _allowedTokens.TryRemove(prevTokenId, out removedValue);
                            _userSessions.TryRemove(tokenId, out removedValue);

                            _allowedTokens.TryAdd(tokenId, token);
                            Console.WriteLine($"Using refreshed token: {prevTokenId}:{tokenId}");
                        }


                        if (!_allowedTokens.ContainsKey(tokenId))
                        {
                            Console.WriteLine($"Use of revoked Token: {tokenId} blocked.");
                            await Prepare401Response(
                                $"Unauthorized use of revoked token ID: {tokenId}.",
                                context
                            );
                            return;
                        }

                        if (context.Request.GetDisplayUrl().Contains("Logout"))
                        {
                            _allowedTokens.TryRemove(tokenId, out removedValue);
                            _userSessions.TryRemove(tokenId, out removedValue);

                            Console.WriteLine($"Revoked token: {prevTokenId}:{tokenId}");
                        }
#if DEBUG
                        Console.WriteLine("API-URL: " + context.Request.GetDisplayUrl());
                        _allowedTokens
                            .Select(
                                i => $"{i.Key} => ...{i.Value[(i.Value.Length - 8)..i.Value.Length]}"
                            )
                            .ToList()
                            .ForEach(Console.WriteLine);
#endif
                        Console.WriteLine("Allowed Tokens: " + _allowedTokens.Count + " Current Token: " + tokenId);

                        string expiration = principal.Claims.FirstOrDefault(
                                c => c.Type == JwtRegisteredClaimNames.Exp
                            )?.Value ?? "0";

                        if (expiration != "0")
                        {
                            long remainingTime = long.Parse(expiration) - DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                            context.Response.Headers.Append(
                                "Remaining-Time",
                                remainingTime.ToString()
                            );
                            context.Response.Headers.Append(
                                "Access-Control-Expose-Headers",
                                "Remaining-Time"
                            );
                        }
                        context.Items["userclaimmodel"] = jwtAuthClaimModel;
                        //RemoveExpiredTokens();
                        await _next(context);
                    }
                    else
                    {
                        await Prepare401Response(
                            $"TokenID: {tokenId} is not valid.",
                            context
                        );
                        return;
                    }
                }
                else
                {
                    //Not possible to authenticate without a toekn
                    await _next(context);
                }
            }
            //catch (SecurityTokenNotYetValidException ex)
            //{
            //    string message = "Token is not activated yet.";

            //    message = "TokenId: " + tokenId + " is not activated yet: " + ex.InnerException?.ToString() + ex.ToString();

            //    await Prepare401Response(
            //        message,
            //        context
            //    );
            //    return;
            //}
            //catch (SecurityTokenExpiredException ex)
            //{
            //    string message = "Token is already expired.";

            //    message = "TokenId: " + tokenId + " expired: " + ex.InnerException?.ToString() + ex.ToString();

            //    await Prepare401Response(
            //        message,
            //        context
            //    );
            //    return;
            //}

            catch (SecurityTokenValidationException ex)
            {
                string message;

                // Check for expired token based on exception message content
                if (ex.Message?.ToLowerInvariant().Contains("expired") == true)
                {
                    // Token expired case
                    message = "Token is already expired.";
#if DEBUG
                    message += " TokenId: " + tokenId + " expired: " + ex.InnerException?.ToString() + ex.ToString();
#endif
                }
                else
                {
                    // Token not activated yet case
                    message = "Token is not activated yet.";
#if DEBUG
                    message += " TokenId: " + tokenId + " is not activated yet: " + ex.InnerException?.ToString() + ex.ToString();
#endif
                }

                await Prepare401Response(message, context);
                return;
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                response.Message = "Server Error: check server log for details.";
                Console.WriteLine("Server Error: check server log for details." + ex.ToString());
#if DEBUG
                response.Message = "Server Error TokenId: " + tokenId
                    + ", " + ex.InnerException?.ToString() + ex.ToString();
#endif
                response.ResponseStatus = APIResponseStatus.Error;
                response.Result = false;
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.Response.ContentType = "application/json; charset=utf-8";
                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
                return;
            }

        }

    }
    public static class AuthTokenMiddlewareExtensions
    {
        public static IApplicationBuilder UseAuthTokenMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuthTokenMiddleware>();
        }
    }

}
