using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using sjam.Auth;
using sjam.Dal;
using sjam.Extensions;
using sjam.Helpers;
using sjam.Middleware;
using sjam.RabbitMQ.Common;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Reflection;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Assign configuration from builder.Configuration before use
IConfiguration configuration = builder.Configuration;

#region Database Connections
Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

/*------ Database Connection ------*/
builder.Services.AddDbContext<EFContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DBConnection"),
        npgsqlOptions => npgsqlOptions.EnableRetryOnFailure(
            10,
            TimeSpan.FromSeconds(5),
            null)));
#endregion

// Application Services
builder.Services.AddApplicationServices();
// Repository Services
builder.Services.AddRepositoryServices();
// RabbitMQ Services
builder.Services.AddRabbitMQServices(builder.Configuration);

//builder.Services
//   .AddRabbitMQ(builder.Configuration)
//   .AddMessageProcessing();

builder.Services
    .AddRabbitMQ(builder.Configuration)
    .AddMessageProcessing(builder.Configuration);


builder.Services.AddHealthChecks();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(5);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});


builder.Services.AddControllers(options =>
{
    options.Filters.AddService<ApiConfigFilter>();
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.WriteIndented = true;
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();
builder.Services.AddJwtAuthentication();
builder.Services.AddAuthorizationPolicies();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1",
        new OpenApiInfo
        {
            Title = "STATE JIT - version_1.0.0",
            Version = "v1",
            Description = "STATE JIT Web API",
        }
    );
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 1safsfsdfdfd\"",
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference {
                    Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                }
            },
            new string[] {}
        }
    });

    options.MapType<IFormFile>(() => new OpenApiSchema
    {
        Type = "string",
        Format = "binary"
    });
});

builder.Services.AddMvc(options =>
{
    options.SuppressAsyncSuffixInActionNames = false;
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("RestrictedPolicy", policy =>
    {
        policy.WithOrigins("https://ifms.wb.gov.in", "https://train-ifms.wb.gov.in")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });

    options.AddPolicy("AllowAllPolicy", policy =>
    {
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// AutoMapper
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<MapperClass>();
});

var app = builder.Build();

app.UseHsts();
app.Use(async (context, next) =>
{
    context.Response.Headers.Remove("Server");
    context.Response.Headers.Remove("X-Powered-By");
    context.Response.Headers.Remove("X-AspNet-Version");
    context.Response.Headers.Remove("X-AspNetMvc-Version");
    await next();
});

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
app.MapHealthChecks("/health");
//app.MapOpenApi();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("STATE JIT API")
            .WithTheme(ScalarTheme.DeepSpace)
            .WithOpenApiRoutePattern("/swagger/{documentName}/swagger.json")
            .WithDefaultHttpClient(
                ScalarTarget.CSharp,
                ScalarClient.HttpClient)
            .HideModels();
    });
}

// Use CORS based on environment
app.UseCors(app.Environment.IsDevelopment() ? "AllowAllPolicy" : "RestrictedPolicy");

app.UseRouting();
app.UseSession();

app.UseStaticFiles();

app.UseDefaultFiles();

app.UseHttpsRedirection();

app.UseAuditLog();
app.UseAuthentication();
app.UseAuthorization();
app.UseAuthTokenMiddleware();

app.MapControllers();

app.Run();
