using Microsoft.AspNetCore.Http;
using Swasthya.CoreLabs.Api.Security;
using Swasthya.CoreLabs.Application.Abstractions;
using Swasthya.CoreLabs.Application.Models;
using Swasthya.CoreLabs.Application.Services;

namespace Swasthya.CoreLabs.Api.Endpoints;

public static class OrderEndpoints
{
    public static IEndpointRouteBuilder MapOrderEndpoints(
        this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app
            .MapGroup("/v1/organizations/{organizationId:guid}/orders")
            .WithTags("Orders");

        group.AddEndpointFilter(async (context, next) =>
        {
            object? result = await next(context);
            var unitOfWork = context.HttpContext.RequestServices
                .GetRequiredService<IUnitOfWork>();
            await unitOfWork.SaveChangesAsync(context.HttpContext.RequestAborted);
            return result;
        });

        group.RequireAuthorization(IdentityTenancyEndpoints.Policy);

        MapOrderEndpointsCore(group);

        return app;
    }

    private static void MapOrderEndpointsCore(RouteGroupBuilder group)
    {
        group.MapPost("", async (
                HttpContext httpContext,
                Guid organizationId,
                CreateOrderRequest request,
                OrderService service,
                CancellationToken cancellationToken) =>
            {
                CreateOrderResultDto result = await service.CreateOrderAsync(
                    httpContext.GetAuthContext(), organizationId, request!, cancellationToken);
                return result.WasReplayed
                    ? Results.Ok(result.Order)
                    : Results.Created(
                        $"/v1/organizations/{organizationId}/orders/{result.Order.OrderNumber}",
                        result.Order);
            });

        group.MapGet("", async (
                HttpContext httpContext,
                Guid organizationId,
                OrderService service,
                CancellationToken cancellationToken,
                Guid? facilityId = null) =>
                await service.ListOrdersAsync(
                    httpContext.GetAuthContext(), organizationId, facilityId, cancellationToken));

        group.MapGet("/{orderNumber}", async (
                HttpContext httpContext,
                Guid organizationId,
                string orderNumber,
                OrderService service,
                CancellationToken cancellationToken) =>
                await service.GetOrderAsync(
                    httpContext.GetAuthContext(), organizationId, orderNumber, cancellationToken));

        group.MapPut("/{orderNumber}", async (
                HttpContext httpContext,
                Guid organizationId,
                string orderNumber,
                UpdateOrderRequest request,
                OrderService service,
                CancellationToken cancellationToken) =>
                await service.UpdateOrderAsync(
                    httpContext.GetAuthContext(), organizationId, orderNumber, request!, cancellationToken));

        group.MapPost("/{orderNumber}/items", async (
                HttpContext httpContext,
                Guid organizationId,
                string orderNumber,
                AddOrderItemRequest request,
                OrderService service,
                CancellationToken cancellationToken) =>
                await service.AddOrderItemAsync(
                    httpContext.GetAuthContext(), organizationId, orderNumber, request!, cancellationToken));

        group.MapPost("/{orderNumber}/items/{orderItemId:guid}/transitions/cancel", async (
                HttpContext httpContext,
                Guid organizationId,
                string orderNumber,
                Guid orderItemId,
                OrderService service,
                CancellationToken cancellationToken) =>
                await service.CancelOrderItemAsync(
                    httpContext.GetAuthContext(), organizationId, orderNumber, orderItemId, cancellationToken));

        group.MapPost("/{orderNumber}/transitions/cancel", async (
                HttpContext httpContext,
                Guid organizationId,
                string orderNumber,
                OrderService service,
                CancellationToken cancellationToken) =>
                await service.CancelOrderAsync(
                    httpContext.GetAuthContext(), organizationId, orderNumber, cancellationToken));
    }
}
