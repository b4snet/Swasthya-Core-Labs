using Swasthya.CoreLabs.Domain.Specimen;

namespace Swasthya.CoreLabs.Domain.Specimen;

/// <summary>
/// An accession is the laboratory operational identifier/grouping used to
/// process a specimen or set of specimens.  An accession is created when
/// specimens are received and grouped together for processing; it has its
/// own lifecycle independent of the individual specimen lifecycles.  Multiple
/// specimens may belong to a single accession, or a single specimen may be
/// part of multiple accessions where the laboratory's workflow requires it.
/// </summary>
public sealed class Accession
{
    public Guid Id { get; private set; }

    public Guid FacilityId { get; private set; }

    public string? AccessionNumber { get; private set; }

    public string? Description { get; private set; }

    public Guid? ReceivingActorId { get; private set; }

    public AccessionStatus Status { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public Accession()
    {
    }

    public Accession(
        Guid facilityId,
        string accessionNumber,
        string? description = null,
        Guid? receivingActorId = null)
    {
        if (facilityId == Guid.Empty)
        {
            throw new ArgumentException("A facility identifier is required.", nameof(facilityId));
        }

        if (string.IsNullOrWhiteSpace(accessionNumber))
        {
            throw new ArgumentException("An accession number is required.", nameof(accessionNumber));
        }

        Id = Guid.NewGuid();
        FacilityId = facilityId;
        AccessionNumber = accessionNumber;
        Description = description;
        ReceivingActorId = receivingActorId;
        Status = AccessionStatus.Created;
        CreatedAtUtc = DateTimeOffset.UtcNow;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void SetStatus(AccessionStatus newStatus)
    {
        AccessionLifecycle.RequireTransition(Status, newStatus);
        Status = newStatus;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void AssociateSpecimen(Guid specimenId)
    {
        // No direct state change; the specimen's FacilityId and status
        // transitions are managed by the specimen and lifecycle logic.
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void SetDescription(string newDescription)
    {
        Description = newDescription;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }
}
