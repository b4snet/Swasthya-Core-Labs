using Swasthya.CoreLabs.Domain.Laboratory;
using Swasthya.CoreLabs.Domain.Orders;
using Swasthya.CoreLabs.Domain.Specimen;

namespace Swasthya.CoreLabs.Domain.Specimen;

/// <summary>
/// A specimen is the actual collected/received biological material that is
/// the subject of a diagnostic test or panel.  A specimen is linked to one
/// or more order items that prompted its collection, and to one container
/// that holds it.  Lifecycle states (Requested → Collected → Received →
/// Accessioned → Rejected/Cancelled/Stored) are enforced through
/// SpecimenLifecycle; invalid transitions throw.
/// </summary>
public sealed class Specimen
{
    public Guid Id { get; private set; }

    public Guid OrderItemId { get; private set; }

    public Guid ContainerId { get; private set; }

    public SpecimenType SpecimenType { get; private set; }

    public string? AccessionNumber { get; private set; }

    public SpecimenStatus Status { get; private set; }

    public string? CollectorIdentity { get; private set; }

    public Guid? FacilityId { get; private set; }

    public string? CollectionNotes { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    private Specimen()
    {
    }

    public Specimen(
        Guid orderItemId,
        Guid containerId,
        SpecimenType specimenType,
        string? accessionNumber = null,
        SpecimenStatus status = SpecimenStatus.Requested,
        string? collectorIdentity = null,
        Guid? facilityId = null,
        string? collectionNotes = null)
    {
        if (facilityId.HasValue && facilityId.Value == Guid.Empty)
        {
            throw new ArgumentException("Facility identifier must not be empty.", nameof(facilityId));
        }

        Id = Guid.NewGuid();
        OrderItemId = orderItemId;
        ContainerId = containerId;
        SpecimenType = specimenType;
        AccessionNumber = accessionNumber;
        Status = status;
        CollectorIdentity = collectorIdentity;
        FacilityId = facilityId;
        CollectionNotes = collectionNotes;
        CreatedAtUtc = DateTimeOffset.UtcNow;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Transition the specimen to a new lifecycle state.  Only transitions
    /// defined in <see cref="SpecimenLifecycle"/> are allowed; otherwise an
    /// <see cref="InvalidOperationException"/> is thrown.
    /// </summary>
    public void SetStatus(SpecimenStatus newStatus)
    {
        SpecimenLifecycle.RequireTransition(Status, newStatus);
        Status = newStatus;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Associate this specimen with a new container, updating the container
    /// reference and preserving audit integrity.
    /// </summary>
    public void AssignContainer(Guid newContainerId)
    {
        if (newContainerId == Guid.Empty)
        {
            throw new ArgumentException("Container identifier must not be empty.", nameof(newContainerId));
        }

        ContainerId = newContainerId;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Set or update the accession number for this specimen.  When the
    /// accession number is set the specimen status is nudged toward
    /// Accessioned; the caller may explicitly promote the status afterward.
    /// </summary>
    public void SetAccessionNumber(string accessionNumber)
    {
        AccessionNumber = accessionNumber;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Set the collector identity for this specimen.
    /// </summary>
    public void SetCollectorIdentity(string collectorIdentity)
    {
        CollectorIdentity = collectorIdentity;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Set the collection notes for this specimen.
    /// </summary>
    public void SetCollectionNotes(string collectionNotes)
    {
        CollectionNotes = collectionNotes;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Link this specimen to a different order item.  The previous ordering
    /// relationship is cleared and a new one established.
    /// </summary>
    public void ReassignOrderItem(Guid newOrderItemId)
    {
        if (newOrderItemId == Guid.Empty)
        {
            throw new ArgumentException("Order item identifier must not be empty.", nameof(newOrderItemId));
        }

        OrderItemId = newOrderItemId;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }
}
