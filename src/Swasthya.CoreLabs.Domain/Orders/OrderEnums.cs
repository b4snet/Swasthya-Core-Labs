namespace Swasthya.CoreLabs.Domain.Orders;

/// <summary>
/// Lifecycle of a diagnostic order. Deliberately minimal for the Phase 3
/// foundation: an order is Requested when it enters the system and can only be
/// Cancelled. Result/specimen/collection states arrive in later phases.
/// </summary>
public enum OrderStatus
{
    Requested = 0,
    Cancelled = 1,
}

/// <summary>
/// Lifecycle of a single line item on an order.
/// </summary>
public enum OrderItemStatus
{
    Requested = 0,
    Cancelled = 1,
}

/// <summary>
/// Clinical urgency of an order or line item.
/// </summary>
public enum OrderPriority
{
    Routine = 0,
    Urgent = 1,
    Stat = 2,
}
