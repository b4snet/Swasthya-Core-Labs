using Microsoft.AspNetCore.Http;
using Swasthya.CoreLabs.Api.Security;
using Swasthya.CoreLabs.Application.Abstractions;
using Swasthya.CoreLabs.Application.Models;
using Swasthya.CoreLabs.Application.Services;

namespace Swasthya.CoreLabs.Api.Endpoints;

public static class SpecimenEndpoints
{
    public static IEndpointRouteBuilder MapSpecimenEndpoints(
        this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app
            .MapGroup("/v1/organizations/{organizationId:guid}/specimens")
            .WithTags("Specimens");

        group.AddEndpointFilter(async (context, next) =>
        {
            object? result = await next(context);
            var unitOfWork = context.HttpContext.RequestServices
                .GetRequiredService<IUnitOfWork>();
            await unitOfWork.SaveChangesAsync(context.HttpContext.RequestAborted);
            return result;
        });

        group.RequireAuthorization(IdentityTenancyEndpoints.Policy);

        MapSpecimenEndpointsCore(group);

        return app;
    }

    private static void MapSpecimenEndpointsCore(RouteGroupBuilder group)
    {
        group.MapPost("", async (
                    HttpContext httpContext,
                    Guid organizationId,
                    CreateSpecimenRequest request,
                    SpecimenService service,
                    CancellationToken cancellationToken) =>
                {
                    var result = await service.CreateSpecimenAsync(
                        httpContext.GetAuthContext(), organizationId, request!, cancellationToken);
                    return Results.Created(
                        $"/v1/organizations/{organizationId}/specimens/{result.Id}",
                        result);
                });

        group.MapGet("", async (
                HttpContext httpContext,
                Guid organizationId,
                SpecimenService service,
                CancellationToken cancellationToken,
                Guid? facilityId = null) =>
                await service.ListSpecimensAsync(
                    httpContext.GetAuthContext(), organizationId, facilityId, cancellationToken));

        group.MapGet("/{specimenNumber}", async (
                HttpContext httpContext,
                Guid organizationId,
                string specimenNumber,
                SpecimenService service,
                CancellationToken cancellationToken) =>
                await service.GetSpecimenAsync(
                    specimenNumber, cancellationToken));

        group.MapPut("/{specimenNumber}", async (
                HttpContext httpContext,
                Guid organizationId,
                string specimenNumber,
                UpdateSpecimenRequest request,
                SpecimenService service,
                CancellationToken cancellationToken) =>
                await service.UpdateSpecimenAsync(
                    specimenNumber, request!, cancellationToken));

        group.MapPost("/{specimenNumber}/collection", async (
                HttpContext httpContext,
                Guid organizationId,
                string specimenNumber,
                CreateCollectionRequest request,
                SpecimenService service,
                CancellationToken cancellationToken) =>
                await service.RecordCollectionAsync(
                    specimenNumber, request!, cancellationToken));

        group.MapPost("/{specimenNumber}/accession", async (
                HttpContext httpContext,
                Guid organizationId,
                string specimenNumber,
                CreateAccessionRequest request,
                SpecimenService service,
                CancellationToken cancellationToken) =>
                await service.RecordAccessionAsync(
                    specimenNumber, request!, cancellationToken));
    }
}
