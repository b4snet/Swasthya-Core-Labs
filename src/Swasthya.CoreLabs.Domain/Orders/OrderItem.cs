using Swasthya.CoreLabs.Domain.Laboratory;

namespace Swasthya.CoreLabs.Domain.Orders;

/// <summary>
/// A line item on a diagnostic order. An item references exactly one test or
/// one panel and pins the catalog version in force at request time, plus a
/// snapshot of its code and name so the request is stable even if the catalog
/// evolves afterwards (see ADR-0016). A panel is ordered as a single item;
/// member expansion is deferred to Phase 4+.
/// </summary>
public sealed class OrderItem
{
    private OrderItem()
    {
    }

    private OrderItem(
        Guid orderId,
        int sequenceNumber,
        Guid? testId,
        Guid? panelId,
        Guid? testVersionId,
        Guid? panelVersionId,
        string? testCode,
        string? testName,
        string? panelCode,
        string? panelName,
        OrderPriority? priority,
        decimal? requestedQuantity,
        string? quantityUnitUcumCode)
    {
        if (orderId == Guid.Empty)
        {
            throw new ArgumentException("An order is required.", nameof(orderId));
        }

        if (sequenceNumber < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(sequenceNumber),
                sequenceNumber,
                "Sequence number must not be negative.");
        }

        bool hasTest = testId is not null
            && testVersionId is not null
            && testCode is not null
            && testName is not null;
        bool hasPanel = panelId is not null
            && panelVersionId is not null
            && panelCode is not null
            && panelName is not null;

        if (hasTest == hasPanel)
        {
            throw new ArgumentException(
                "An order item must reference exactly one test or panel with its version.",
                nameof(testId));
        }

        if (priority is { } value && !Enum.IsDefined(value))
        {
            throw new ArgumentOutOfRangeException(
                nameof(priority),
                priority,
                "Unknown order item priority.");
        }

        Id = Guid.NewGuid();
        OrderId = orderId;
        SequenceNumber = sequenceNumber;
        TestId = testId;
        PanelId = panelId;
        TestVersionId = testVersionId;
        PanelVersionId = panelVersionId;
        TestCode = testCode;
        TestName = testName;
        PanelCode = panelCode;
        PanelName = panelName;
        Priority = priority;
        Status = OrderItemStatus.Requested;
        SetQuantity(requestedQuantity, quantityUnitUcumCode);
        CreatedAtUtc = DateTimeOffset.UtcNow;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public static OrderItem ForTest(
        Guid orderId,
        int sequenceNumber,
        Guid testId,
        Guid testVersionId,
        string testCode,
        string testName,
        OrderPriority? priority = null,
        decimal? requestedQuantity = null,
        string? quantityUnitUcumCode = null)
    {
        if (testVersionId == Guid.Empty)
        {
            throw new ArgumentException("A test version is required.", nameof(testVersionId));
        }

        return new OrderItem(
            orderId,
            sequenceNumber,
            testId,
            null,
            testVersionId,
            null,
            OrderCodes.RequireSnapshotCode(testCode, nameof(testCode)),
            OrderCodes.RequireSnapshotName(testName, nameof(testName)),
            null,
            null,
            priority,
            requestedQuantity,
            quantityUnitUcumCode);
    }

    public static OrderItem ForPanel(
        Guid orderId,
        int sequenceNumber,
        Guid panelId,
        Guid panelVersionId,
        string panelCode,
        string panelName,
        OrderPriority? priority = null,
        decimal? requestedQuantity = null,
        string? quantityUnitUcumCode = null)
    {
        if (panelVersionId == Guid.Empty)
        {
            throw new ArgumentException("A panel version is required.", nameof(panelVersionId));
        }

        return new OrderItem(
            orderId,
            sequenceNumber,
            null,
            panelId,
            null,
            panelVersionId,
            null,
            null,
            OrderCodes.RequireSnapshotCode(panelCode, nameof(panelCode)),
            OrderCodes.RequireSnapshotName(panelName, nameof(panelName)),
            priority,
            requestedQuantity,
            quantityUnitUcumCode);
    }

    public Guid Id { get; private set; }

    public Guid OrderId { get; private set; }

    public int SequenceNumber { get; private set; }

    public Guid? TestId { get; private set; }

    public Guid? PanelId { get; private set; }

    public Guid? TestVersionId { get; private set; }

    public Guid? PanelVersionId { get; private set; }

    public string? TestCode { get; private set; }

    public string? TestName { get; private set; }

    public string? PanelCode { get; private set; }

    public string? PanelName { get; private set; }

    public OrderItemStatus Status { get; private set; }

    public OrderPriority? Priority { get; private set; }

    public decimal? RequestedQuantity { get; private set; }

    public string? QuantityUnitUcumCode { get; private set; }

    public DateTimeOffset? CancelledAtUtc { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public uint Version { get; private set; }

    public void Cancel()
    {
        OrderItemLifecycle.RequireTransition(Status, OrderItemStatus.Cancelled);
        Status = OrderItemStatus.Cancelled;
        CancelledAtUtc = DateTimeOffset.UtcNow;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    private void SetQuantity(decimal? requestedQuantity, string? quantityUnitUcumCode)
    {
        if (requestedQuantity is null != (quantityUnitUcumCode is null))
        {
            throw new ArgumentException(
                "Requested quantity and its UCUM unit must be provided together.",
                nameof(requestedQuantity));
        }

        if (requestedQuantity is < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(requestedQuantity),
                requestedQuantity,
                "Requested quantity must not be negative.");
        }

        RequestedQuantity = requestedQuantity;
        QuantityUnitUcumCode = quantityUnitUcumCode is null
            ? null
            : CatalogCodes.RequireUcumCode(quantityUnitUcumCode, nameof(quantityUnitUcumCode));
    }
}
