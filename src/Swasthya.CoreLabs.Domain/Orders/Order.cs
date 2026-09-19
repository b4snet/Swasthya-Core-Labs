namespace Swasthya.CoreLabs.Domain.Orders;

/// <summary>
/// A diagnostic order: the facility that will fulfil it and the identity of
/// the subject as an external reference. Patient and encounter references are
/// identifiers owned by the ordering system; this system never stores patient
/// identity data itself. The order holds the mutable lifecycle state; what was
/// requested is captured immutably on <see cref="OrderItem"/> rows that pin the
/// catalog versions in force at request time.
/// </summary>
public sealed class Order
{
    private Order()
    {
    }

    public Order(
        Guid organizationId,
        Guid facilityId,
        string orderNumber,
        string? externalOrderId,
        string patientExternalSystem,
        string patientExternalIdentifier,
        string? encounterExternalSystem = null,
        string? encounterExternalIdentifier = null,
        OrderPriority priority = OrderPriority.Routine,
        Guid? requestedByPrincipalId = null)
    {
        if (organizationId == Guid.Empty)
        {
            throw new ArgumentException("An organization is required.", nameof(organizationId));
        }

        if (facilityId == Guid.Empty)
        {
            throw new ArgumentException("A facility is required.", nameof(facilityId));
        }

        bool hasEncounterSystem = !string.IsNullOrWhiteSpace(encounterExternalSystem);
        bool hasEncounterIdentifier = !string.IsNullOrWhiteSpace(encounterExternalIdentifier);
        if (hasEncounterSystem != hasEncounterIdentifier)
        {
            throw new ArgumentException(
                "Encounter external system and identifier must be provided together.",
                nameof(encounterExternalSystem));
        }

        if (!Enum.IsDefined(priority))
        {
            throw new ArgumentOutOfRangeException(
                nameof(priority),
                priority,
                "Unknown order priority.");
        }

        Id = Guid.NewGuid();
        OrganizationId = organizationId;
        FacilityId = facilityId;
        OrderNumber = OrderCodes.RequireOrderNumber(orderNumber, nameof(orderNumber));
        ExternalOrderId = OrderCodes.NormalizeOptional(externalOrderId, 200, nameof(externalOrderId));
        PatientExternalSystem = OrderCodes.RequireExternalSystem(
            patientExternalSystem, nameof(patientExternalSystem));
        PatientExternalIdentifier = OrderCodes.RequireExternalIdentifier(
            patientExternalIdentifier, nameof(patientExternalIdentifier));
        EncounterExternalSystem = OrderCodes.NormalizeOptional(
            encounterExternalSystem, 100, nameof(encounterExternalSystem));
        EncounterExternalIdentifier = OrderCodes.NormalizeOptional(
            encounterExternalIdentifier, 200, nameof(encounterExternalIdentifier));
        Priority = priority;
        Status = OrderStatus.Requested;
        RequestedByPrincipalId = requestedByPrincipalId;
        RequestedAtUtc = DateTimeOffset.UtcNow;
        CreatedAtUtc = DateTimeOffset.UtcNow;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }

    public Guid OrganizationId { get; private set; }

    public Guid FacilityId { get; private set; }

    public string OrderNumber { get; private set; } = string.Empty;

    public string? ExternalOrderId { get; private set; }

    public string PatientExternalSystem { get; private set; } = string.Empty;

    public string PatientExternalIdentifier { get; private set; } = string.Empty;

    public string? EncounterExternalSystem { get; private set; }

    public string? EncounterExternalIdentifier { get; private set; }

    public OrderPriority Priority { get; private set; }

    public OrderStatus Status { get; private set; }

    public Guid? RequestedByPrincipalId { get; private set; }

    public DateTimeOffset RequestedAtUtc { get; private set; }

    public DateTimeOffset? CancelledAtUtc { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public uint Version { get; private set; }

    /// <summary>
    /// Cancels the order. This is used when the ordering system revokes a
    /// request that never entered specimen collection.
    /// </summary>
    public void Cancel()
    {
        OrderLifecycle.RequireTransition(Status, OrderStatus.Cancelled);
        Status = OrderStatus.Cancelled;
        CancelledAtUtc = DateTimeOffset.UtcNow;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Replaces the order request details while the order is still Requested.
    /// The order number and identities are immutable.
    /// </summary>
    public void Update(
        OrderPriority priority,
        string patientExternalSystem,
        string patientExternalIdentifier,
        string? encounterExternalSystem,
        string? encounterExternalIdentifier)
    {
        if (Status == OrderStatus.Cancelled)
        {
            throw new InvalidOperationException("A cancelled order cannot be modified.");
        }

        bool hasEncounterSystem = !string.IsNullOrWhiteSpace(encounterExternalSystem);
        bool hasEncounterIdentifier = !string.IsNullOrWhiteSpace(encounterExternalIdentifier);
        if (hasEncounterSystem != hasEncounterIdentifier)
        {
            throw new ArgumentException(
                "Encounter external system and identifier must be provided together.",
                nameof(encounterExternalSystem));
        }

        if (!Enum.IsDefined(priority))
        {
            throw new ArgumentOutOfRangeException(
                nameof(priority),
                priority,
                "Unknown order priority.");
        }

        PatientExternalSystem = OrderCodes.RequireExternalSystem(
            patientExternalSystem, nameof(patientExternalSystem));
        PatientExternalIdentifier = OrderCodes.RequireExternalIdentifier(
            patientExternalIdentifier, nameof(patientExternalIdentifier));
        EncounterExternalSystem = OrderCodes.NormalizeOptional(
            encounterExternalSystem, 100, nameof(encounterExternalSystem));
        EncounterExternalIdentifier = OrderCodes.NormalizeOptional(
            encounterExternalIdentifier, 200, nameof(encounterExternalIdentifier));
        Priority = priority;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }
}
