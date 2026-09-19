using Microsoft.AspNetCore.Http;
using Swasthya.CoreLabs.Api.Security;
using Swasthya.CoreLabs.Application.Services;
using Swasthya.CoreLabs.Application.Abstractions;
using Swasthya.CoreLabs.Application.Models;
using Swasthya.CoreLabs.Domain.Common;
using Swasthya.CoreLabs.Domain.Orders;
using Swasthya.CoreLabs.Domain.Specimen;

namespace Swasthya.CoreLabs.Api.Endpoints;

public static class WorklistEndpoints
{
    public static IEndpointRouteBuilder MapWorklistEndpoints(
        this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app
            .MapGroup("/v1/organizations/{organizationId:guid}/worklist")
            .WithTags("Worklist");

        group.AddEndpointFilter(async (context, next) =>
        {
            object? result = await next(context);
            var unitOfWork = context.HttpContext.RequestServices
                .GetRequiredService<IUnitOfWork>();
            await unitOfWork.SaveChangesAsync(context.HttpContext.RequestAborted);
            return result;
        });

        group.RequireAuthorization(IdentityTenancyEndpoints.Policy);

        MapWorklistEndpointsCore(group);

        return app;
    }

    private static void MapWorklistEndpointsCore(RouteGroupBuilder group)
    {
        group.MapGet("", async (
                HttpContext httpContext,
                Guid organizationId,
                WorklistService worklistService,
                CancellationToken cancellationToken,
                Guid? facilityId = null,
                string? status = null,
                string? accessionNumber = null,
                Guid? orderItemId = null) =>
            {
                var specimens = await worklistService.ListSpecimensAsWorklistAsync(
                    httpContext.GetAuthContext(), organizationId, facilityId, status, accessionNumber, orderItemId, cancellationToken);

                return Results.Ok(specimens);
            });

        group.MapGet("/accessions", async (
                HttpContext httpContext,
                Guid organizationId,
                WorklistService worklistService,
                CancellationToken cancellationToken,
                Guid? facilityId = null,
                string? status = null,
                string? accessionNumber = null) =>
            {
                var accessions = await worklistService.ListAccessionsAsWorklistAsync(
                    httpContext.GetAuthContext(), organizationId, facilityId, status, accessionNumber, cancellationToken);

                return Results.Ok(accessions);
            });

        group.MapGet("/order-items", async (
                HttpContext httpContext,
                Guid organizationId,
                WorklistService worklistService,
                CancellationToken cancellationToken,
                Guid? facilityId = null,
                string? status = null,
                Guid? orderId = null) =>
            {
                var orderItems = await worklistService.ListOrderItemsAsWorklistAsync(
                    httpContext.GetAuthContext(), organizationId, facilityId, status, orderId, cancellationToken);

                return Results.Ok(orderItems);
            });

        group.MapGet("/specimen-types", async (
                HttpContext httpContext,
                Guid organizationId,
                WorklistService worklistService,
                CancellationToken cancellationToken) =>
            {
                var types = await worklistService.GetAvailableSpecimenTypesAsync(organizationId, cancellationToken);

                return Results.Ok(types);
            });
    }
}
