using Swasthya.CoreLabs.Domain.Orders;

namespace Swasthya.CoreLabs.Application.Abstractions;

public interface IOrderRepository
{
    Task<Order?> FindOrderByNumberAsync(
        Guid organizationId,
        string orderNumber,
        CancellationToken cancellationToken);

    Task<Order?> FindOrderByExternalIdAsync(
        Guid organizationId,
        string externalOrderId,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Order>> ListOrdersAsync(
        Guid organizationId,
        IReadOnlyCollection<Guid>? facilityIds,
        CancellationToken cancellationToken);

    Task AddOrderAsync(Order order, CancellationToken cancellationToken);

    Task<OrderItem?> FindOrderItemAsync(
        Guid orderId,
        Guid orderItemId,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<OrderItem>> ListOrderItemsAsync(
        IReadOnlyCollection<Guid> orderIds,
        CancellationToken cancellationToken);

    Task AddOrderItemAsync(OrderItem item, CancellationToken cancellationToken);
}
