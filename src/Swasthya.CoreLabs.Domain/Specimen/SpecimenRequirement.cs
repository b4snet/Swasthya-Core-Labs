using Swasthya.CoreLabs.Domain.Laboratory;

namespace Swasthya.CoreLabs.Domain.Specimen;

/// <summary>
/// A specimen requirement links an order item to the kind of specimen
/// that must be collected to perform the requested diagnostic service.
/// An order item may have zero, one, or multiple requirements; a specimen
/// may satisfy requirements from one or more order items where clinically
/// / operationally justified.
/// </summary>
public sealed class SpecimenRequirement
{
    private SpecimenRequirement()
    {
    }

    private SpecimenRequirement(
        Guid orderItemId,
        Guid specimenId,
        int sequenceNumber,
        SpecimenType specimenType,
        string? description = null)
    {
        if (orderItemId == Guid.Empty)
        {
            throw new ArgumentException("An order item is required.", nameof(orderItemId));
        }

        if (specimenId == Guid.Empty)
        {
            throw new ArgumentException("A specimen is required.", nameof(specimenId));
        }

        if (sequenceNumber < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(sequenceNumber),
                sequenceNumber,
                "Sequence number must not be negative.");
        }

        OrderItemId = orderItemId;
        SpecimenId = specimenId;
        SequenceNumber = sequenceNumber;
        SpecimenType = specimenType;
        Description = description;
        CreatedAtUtc = DateTimeOffset.UtcNow;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }

    public Guid OrderItemId { get; private set; }

    public Guid SpecimenId { get; private set; }

    public int SequenceNumber { get; private set; }

    public SpecimenType SpecimenType { get; private set; }

    public string? Description { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static SpecimenRequirement Create(
        Guid orderItemId,
        Guid specimenId,
        int sequenceNumber,
        SpecimenType specimenType,
        string? description = null)
    {
        return new SpecimenRequirement(orderItemId, specimenId, sequenceNumber, specimenType, description);
    }
}
