using Microsoft.EntityFrameworkCore;
using Swasthya.CoreLabs.Application.Abstractions;
using Swasthya.CoreLabs.Domain.Orders;

namespace Swasthya.CoreLabs.Infrastructure.Persistence;

public sealed class OrderRepository(CoreLabDbContext dbContext) : IOrderRepository
{
    public Task<Order?> FindOrderByNumberAsync(
        Guid organizationId,
        string orderNumber,
        CancellationToken cancellationToken) =>
        dbContext.Orders
            .FirstOrDefaultAsync(
                o => o.OrganizationId == organizationId && o.OrderNumber == orderNumber,
                cancellationToken);

    public Task<Order?> FindOrderByExternalIdAsync(
        Guid organizationId,
        string externalOrderId,
        CancellationToken cancellationToken) =>
        dbContext.Orders
            .FirstOrDefaultAsync(
                o => o.OrganizationId == organizationId && o.ExternalOrderId == externalOrderId,
                cancellationToken);

    public async Task<IReadOnlyCollection<Order>> ListOrdersAsync(
        Guid organizationId,
        IReadOnlyCollection<Guid>? facilityIds,
        CancellationToken cancellationToken)
    {
        IQueryable<Order> query = dbContext.Orders.AsNoTracking()
            .Where(o => o.OrganizationId == organizationId);

        if (facilityIds is not null)
        {
            query = query.Where(o => facilityIds.Contains(o.FacilityId));
        }

        return await query
            .OrderBy(o => o.OrderNumber)
            .ThenBy(o => o.FacilityId)
            .ToListAsync(cancellationToken);
    }

    public Task AddOrderAsync(Order order, CancellationToken cancellationToken)
    {
        dbContext.Orders.Add(order);
        return Task.CompletedTask;
    }

    public Task<OrderItem?> FindOrderItemAsync(
        Guid orderId,
        Guid orderItemId,
        CancellationToken cancellationToken) =>
        dbContext.OrderItems
            .FirstOrDefaultAsync(
                i => i.OrderId == orderId && i.Id == orderItemId,
                cancellationToken);

    public async Task<IReadOnlyCollection<OrderItem>> ListOrderItemsAsync(
        IReadOnlyCollection<Guid> orderIds,
        CancellationToken cancellationToken)
    {
        if (orderIds.Count == 0)
        {
            return Array.Empty<OrderItem>();
        }

        return await dbContext.OrderItems
            .Where(i => orderIds.Contains(i.OrderId))
            .OrderBy(i => i.OrderId)
            .ThenBy(i => i.SequenceNumber)
            .ToListAsync(cancellationToken);
    }

    public Task AddOrderItemAsync(OrderItem item, CancellationToken cancellationToken)
    {
        dbContext.OrderItems.Add(item);
        return Task.CompletedTask;
    }
}
