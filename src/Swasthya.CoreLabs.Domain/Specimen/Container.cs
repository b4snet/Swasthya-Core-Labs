using Swasthya.CoreLabs.Domain.Specimen;

namespace Swasthya.CoreLabs.Domain.Specimen;

/// <summary>
/// A container is the physical vessel that holds a specimen.  Each container
/// has a durable internal identifier and an optional operational label (e.g.
/// barcode).  The relationship between specimen and container is one-to-one
/// at creation; a specimen may be reassigned to a different container if
/// required by the lifecycle.
/// </summary>
public sealed class Container
{
    public Guid Id { get; private set; }

    public Guid FacilityId { get; private set; }

    public string? Label { get; private set; }

    public string? Barcode { get; private set; }

    public SpecimenStatus Status { get; private set; }

    public SpecimenType SpecimenType { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    private Container()
    {
    }

    public Container(
        Guid facilityId,
        SpecimenType specimenType,
        string? label = null,
        string? barcode = null,
        SpecimenStatus status = SpecimenStatus.Requested)
    {
        if (facilityId == Guid.Empty)
        {
            throw new ArgumentException("A facility identifier is required.", nameof(facilityId));
        }

        Id = Guid.NewGuid();
        FacilityId = facilityId;
        SpecimenType = specimenType;
        Label = label;
        Barcode = barcode;
        Status = status;
        CreatedAtUtc = DateTimeOffset.UtcNow;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }
}
