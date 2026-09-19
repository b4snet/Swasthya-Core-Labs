using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Swasthya.CoreLabs.Api.Endpoints;
using Swasthya.CoreLabs.Api.Middleware;
using Swasthya.CoreLabs.Api.Options;
using Swasthya.CoreLabs.Api.Security;
using Swasthya.CoreLabs.Application.Abstractions;
using Swasthya.CoreLabs.Application.Authorization;
using Swasthya.CoreLabs.Application.Services;
using Swasthya.CoreLabs.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<AuthOptions>(
    builder.Configuration.GetSection(AuthOptions.SectionName));

builder.Services.AddHttpContextAccessor();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi(options =>
    options.AddDocumentTransformer<BearerSecuritySchemeDocumentTransformer>());
builder.Services.AddHealthChecks();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true,
            }));
});

string? coreLabsConnectionString = builder.Configuration.GetConnectionString("CoreLabs");
bool hasPersistence = !string.IsNullOrWhiteSpace(coreLabsConnectionString);

if (hasPersistence)
{
    builder.Services.AddSwasthyaPersistence(builder.Configuration);
    builder.Services.AddScoped<ICorrelationIdProvider, AmbientCorrelationIdProvider>();
    builder.Services.AddScoped<IAuthContextProvider, AuthContextProvider>();
    builder.Services.AddScoped<OrganizationQueryService>();
    builder.Services.AddScoped<FacilityQueryService>();
    builder.Services.AddScoped<AuditQueryService>();
    builder.Services.AddScoped<AuthContextQueryService>();
    builder.Services.AddScoped<LaboratoryOrganizationQueryService>();
    builder.Services.AddScoped<LaboratoryCatalogQueryService>();
    builder.Services.AddScoped<TestCatalogService>();
    builder.Services.AddScoped<PanelService>();
    builder.Services.AddScoped<ReferenceRangeService>();
    builder.Services.AddScoped<ConfigurationItemService>();
    builder.Services.AddScoped<ResultService>();
    builder.Services.AddScoped<OrderService>();
}

AuthOptions authOptions = builder.Configuration
    .GetSection(AuthOptions.SectionName)
    .Get<AuthOptions>()
    ?? new AuthOptions();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => ConfigureJwtBearer(options, authOptions));

builder.Services.AddAuthorization(options =>
    options.AddPolicy(
        IdentityTenancyEndpoints.Policy,
        policy => policy.AddRequirements(new CoreLabsIdentityRequirement())));
builder.Services.AddSingleton<IAuthorizationHandler, CoreLabsIdentityAuthorizationHandler>();

if (builder.Environment.IsProduction())
{
    string? connectionString = coreLabsConnectionString;
    if (string.IsNullOrWhiteSpace(authOptions.Issuer)
        || string.IsNullOrWhiteSpace(authOptions.Audience))
    {
        throw new InvalidOperationException(
            "Production requires Authentication:Issuer and Authentication:Audience.");
    }

    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException(
            "Production requires a CoreLabs connection string.");
    }

    if (string.IsNullOrWhiteSpace(authOptions.SigningKeyBase64)
        && string.IsNullOrWhiteSpace(authOptions.JwksUrl))
    {
        throw new InvalidOperationException(
            "Production requires Authentication:SigningKeyBase64 or Authentication:JwksUrl.");
    }
}

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapHealthChecks("/health");

app.MapGet("/version-info", () => new
{
    name = "swasthya-core-labs-api",
    version = typeof(Program).Assembly.GetName().Version?.ToString() ?? "0.0.0",
});

if (hasPersistence)
{
    app.MapIdentityTenancyEndpoints();
    app.MapLaboratoryEndpoints();
    app.MapOrderEndpoints();
}

app.Run();

static void ConfigureJwtBearer(JwtBearerOptions options, AuthOptions auth)
{
    options.MapInboundClaims = false;

    var validationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = auth.Issuer,
        ValidateAudience = true,
        ValidAudience = auth.Audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(5),
    };

    if (!string.IsNullOrWhiteSpace(auth.SigningKeyBase64))
    {
        validationParameters.ValidateIssuerSigningKey = true;
        validationParameters.IssuerSigningKey = new SymmetricSecurityKey(
            Convert.FromBase64String(auth.SigningKeyBase64));
    }

    options.TokenValidationParameters = validationParameters;

    if (!string.IsNullOrWhiteSpace(auth.JwksUrl))
    {
        options.MetadataAddress = auth.JwksUrl;
        options.Audience = auth.Audience;
        options.RequireHttpsMetadata =
            auth.JwksUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
    }
}

namespace Swasthya.CoreLabs.Api
{
    public partial class Program
    {
    }
}
