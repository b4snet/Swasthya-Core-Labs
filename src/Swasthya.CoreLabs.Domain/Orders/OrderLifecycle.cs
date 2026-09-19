namespace Swasthya.CoreLabs.Domain.Orders;

/// <summary>
/// Valid transitions for the Phase 3 order lifecycle. Orders are created
/// Requested and can only be Cancelled; Cancelled is terminal.
/// </summary>
public static class OrderLifecycle
{
    public static bool CanTransition(OrderStatus from, OrderStatus to)
    {
        if (from == to)
        {
            return false;
        }

        return from == OrderStatus.Requested && to == OrderStatus.Cancelled;
    }

    public static void RequireTransition(OrderStatus from, OrderStatus to)
    {
        if (!CanTransition(from, to))
        {
            throw new InvalidOperationException(
                $"Order cannot transition from '{from}' to '{to}'.");
        }
    }
}

/// <summary>
/// Valid transitions for order line items in the Phase 3 foundation.
/// </summary>
public static class OrderItemLifecycle
{
    public static bool CanTransition(OrderItemStatus from, OrderItemStatus to)
    {
        if (from == to)
        {
            return false;
        }

        return from == OrderItemStatus.Requested && to == OrderItemStatus.Cancelled;
    }

    public static void RequireTransition(OrderItemStatus from, OrderItemStatus to)
    {
        if (!CanTransition(from, to))
        {
            throw new InvalidOperationException(
                $"Order item cannot transition from '{from}' to '{to}'.");
        }
    }
}
