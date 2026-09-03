using Dapper;
using Microsoft.AspNetCore.Mvc.Controllers;
using Newtonsoft.Json;
using sjam.Bal.Interfaces;
using sjam.Dal;
using System.Data;
using System.Diagnostics;
using System.Text;

namespace sjam.Middleware
{
    public class AuditLogMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuditLogMiddleware> _logger;

        public AuditLogMiddleware(RequestDelegate next, ILogger<AuditLogMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, DapperContext dapperContext, IAuthClaimService authClaimService)
        {
            // Only audit Controller/API requests
            var endpoint = context.GetEndpoint();

            var controllerAction = endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>();

            if (controllerAction == null)
            {
                await _next(context);
                return;
            }

            var user = authClaimService.User;
            var request = context.Request;
            var originalBodyStream = context.Response.Body;

            context.Request.EnableBuffering();

            string requestBody = string.Empty;

            if (request.ContentLength > 0 && request.Body.CanRead)
            {
                using var reader = new StreamReader(
                    request.Body,
                    Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: false,
                    leaveOpen: true);

                requestBody = await reader.ReadToEndAsync();
                request.Body.Position = 0;
            }

            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            Exception exception = null;

            var stopwatch = Stopwatch.StartNew();

            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                exception = ex;
                throw;
            }
            finally
            {
                stopwatch.Stop();
                responseBody.Position = 0;
                var responseText = await new StreamReader(responseBody).ReadToEndAsync();
                responseBody.Position = 0;

                await responseBody.CopyToAsync(originalBodyStream);
                context.Response.Body = originalBodyStream;

                var statusCode = exception != null
                    ? MapExceptionToStatusCode(exception)
                    : context.Response.StatusCode;

                //context.Response.StatusCode = statusCode;

                var exceptionInfo = exception == null ? null : new
                {
                    Type = exception.GetType().FullName,
                    exception.Message,
                    exception.StackTrace
                };

                var otherDetails = JsonConvert.SerializeObject(new
                {
                    Trace = new
                    {
                        context.TraceIdentifier,
                        TimestampUtc = DateTime.UtcNow
                    },

                    User = new
                    {
                        UserId = user?.UserId,
                        UserName = user?.Name,
                        Role = user?.Role,
                        Scope = user?.Scope,
                        FinYear = user?.FinancialYear
                    },

                    Client = new
                    {
                        IpAddress = context.Connection.RemoteIpAddress?.ToString(),
                        UserAgent = request.Headers["User-Agent"].ToString(),
                        Origin = request.Headers["Origin"].ToString(),
                        Referer = request.Headers["Referer"].ToString(),
                        IsHttps = request.IsHttps
                    },

                    Server = new
                    {
                        AppDomain = AppDomain.CurrentDomain.FriendlyName,
                        MachineName = Environment.MachineName,
                        OSVersion = Environment.OSVersion.ToString(),
                        ProcessorCount = Environment.ProcessorCount
                    },

                    Request = new
                    {
                        request.Host,
                        request.Path,
                        request.Method,
                        request.Scheme,
                        request.ContentType,
                        request.QueryString,
                        request.ContentLength
                    },

                    Performance = new
                    {
                        ExecutionTimeMs = stopwatch.ElapsedMilliseconds,
                        ResponseSizeBytes = Encoding.UTF8.GetByteCount(responseText ?? "")
                    },

                    Security = new
                    {
                        StatusCode = statusCode,
                        IsError = exception != null || statusCode >= 400,
                        IsUnauthorized = statusCode == 401,
                        IsForbidden = statusCode == 403
                    },

                    Exception = exceptionInfo
                });

                var finYear = GetFinancialYear();

                var logData = new
                {
                    api_controller = context.GetRouteValue("controller")?.ToString(),
                    api_method = request.Method,
                    api_endpoint = $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}",
                    response_status_code = statusCode,
                    request_data = EnsureValidJson(
                        string.IsNullOrWhiteSpace(requestBody)
                            ? JsonConvert.SerializeObject(request.Query)
                            : requestBody),
                    response_data = EnsureValidJson(responseText),
                    error_details = exceptionInfo == null
                        ? null
                        : JsonConvert.SerializeObject(exceptionInfo),
                    user_details = JsonConvert.SerializeObject(user?.AllClaims ?? new()),
                    ip_address = context.Connection.RemoteIpAddress?.ToString(),
                    created_by = user?.UserId,
                    fin_year = finYear,
                    other_details = otherDetails
                };

                try
                {
                    const string sql = @"
                        INSERT INTO audit.api_activity_log
                        (api_controller, api_method, api_endpoint, response_status_code,
                         request_data, response_data, error_details, user_details,
                         ip_address, created_by, fin_year, other_details)
                        VALUES
                        (@api_controller, @api_method, @api_endpoint, @response_status_code,
                         @request_data::jsonb, @response_data::jsonb, @error_details::jsonb,
                         @user_details::jsonb, @ip_address, @created_by, @fin_year, @other_details::jsonb);";

                    using var connection = dapperContext.CreateAuditLogDBConnection();
                    await EnsureAuditSchemaAsync(connection);
                    await connection.ExecuteAsync(sql, logData);
                }
                catch (Exception logEx)
                {
                    _logger.LogError(logEx, "Audit log failed");
                }
            }
        }

        private static int MapExceptionToStatusCode(Exception ex)
        {
            return ex switch
            {
                ArgumentException => StatusCodes.Status400BadRequest,
                UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                KeyNotFoundException => StatusCodes.Status404NotFound,
                InvalidOperationException => StatusCodes.Status409Conflict,
                NotImplementedException => StatusCodes.Status501NotImplemented,
                TimeoutException => StatusCodes.Status504GatewayTimeout,
                _ => StatusCodes.Status500InternalServerError
            };
        }

        private static string EnsureValidJson(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "{}";

            try
            {
                JsonConvert.DeserializeObject(input);
                return input;
            }
            catch
            {
                return JsonConvert.SerializeObject(new { raw = input });
            }
        }

        private static int GetFinancialYear()
        {
            var today = DateTime.UtcNow;
            int startYear = today.Month >= 4 ? today.Year : today.Year - 1;
            int endYear = startYear + 1;
            return int.Parse($"{startYear % 100:00}{endYear % 100:00}");
        }

        private static async Task EnsureAuditSchemaAsync(IDbConnection connection)
        {
            const string ddl = @"
                CREATE SCHEMA IF NOT EXISTS audit;

                CREATE TABLE IF NOT EXISTS audit.api_activity_log
                (
                    log_id bigserial PRIMARY KEY NOT NULL,
                    api_controller text,
                    api_method text,
                    api_endpoint text,
                    response_status_code integer,
                    request_data jsonb,
                    response_data jsonb,
                    error_details jsonb,
                    user_details jsonb,
                    ip_address text,
                    created_by text,
                    created_at timestamp without time zone DEFAULT now(),
                    fin_year bigint,
                    other_details jsonb
                );
            ";

            await connection.ExecuteAsync(ddl);
        }

    }

    public static class AuditLogMiddlewareExtensions
    {
        public static IApplicationBuilder UseAuditLog(this IApplicationBuilder builder)
            => builder.UseMiddleware<AuditLogMiddleware>();
    }
}
