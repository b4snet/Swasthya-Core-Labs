using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Swasthya.CoreLabs.Api.Security;
using Swasthya.CoreLabs.Application.Services;

namespace Swasthya.CoreLabs.Api.Endpoints;

public static class IdentityTenancyEndpoints
{
    public const string Policy = "identity-tenancy";

    public static IEndpointRouteBuilder MapIdentityTenancyEndpoints(
        this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/v1");

        group.AddEndpointFilter(async (context, next) =>
        {
            object? result = await next(context);
            var unitOfWork = context.HttpContext.RequestServices
                .GetRequiredService<Application.Abstractions.IUnitOfWork>();
            await unitOfWork.SaveChangesAsync(context.HttpContext.RequestAborted);
            return result;
        });

        group.MapGet("/auth/context", async (
                HttpContext httpContext,
                AuthContextQueryService service,
                CancellationToken cancellationToken) =>
                await service.GetAsync(httpContext.GetAuthContext(), cancellationToken))
            .RequireAuthorization(Policy);

        group.MapGet("/organizations", async (
                HttpContext httpContext,
                OrganizationQueryService service,
                CancellationToken cancellationToken) =>
                await service.ListScopedAsync(httpContext.GetAuthContext(), cancellationToken))
            .RequireAuthorization(Policy);

        group.MapGet("/organizations/{organizationId:guid}/facilities", async (
                HttpContext httpContext,
                Guid organizationId,
                FacilityQueryService service,
                CancellationToken cancellationToken) =>
                await service.ListScopedAsync(
                    httpContext.GetAuthContext(),
                    organizationId,
                    cancellationToken))
            .RequireAuthorization(Policy);

        group.MapGet("/audit", async (
                HttpContext httpContext,
                AuditQueryService service,
                CancellationToken cancellationToken,
                int skip = 0,
                int take = 100) =>
                await service.ListScopedAsync(
                    httpContext.GetAuthContext(),
                    skip,
                    take,
                    cancellationToken))
            .RequireAuthorization(Policy);

        return app;
    }
}
