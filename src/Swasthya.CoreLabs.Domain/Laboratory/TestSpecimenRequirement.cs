namespace Swasthya.CoreLabs.Domain.Laboratory;

/// <summary>
/// Operational link between a test concept and the specimen/container needed.
/// Mutable configuration protected by optimistic concurrency.
/// </summary>
public sealed class TestSpecimenRequirement
{
    private TestSpecimenRequirement()
    {
    }

    public TestSpecimenRequirement(
        Guid testId,
        Guid specimenTypeId,
        Guid? specimenSourceId = null,
        Guid? containerTypeId = null,
        decimal? requiredQuantity = null,
        string? quantityUnitUcumCode = null,
        string? notes = null)
    {
        Id = Guid.NewGuid();
        Guid specimenType = specimenTypeId != Guid.Empty
            ? specimenTypeId
            : throw new ArgumentException("A specimen type is required.", nameof(specimenTypeId));

        TestId = testId;
        SpecimenTypeId = specimenType;
        SpecimenSourceId = specimenSourceId;
        ContainerTypeId = containerTypeId;
        SetQuantity(requiredQuantity, quantityUnitUcumCode);
        Notes = CatalogCodes.RequireNotes(notes, nameof(notes));
        CreatedAtUtc = DateTimeOffset.UtcNow;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }

    public Guid TestId { get; private set; }

    public Guid SpecimenTypeId { get; private set; }

    public Guid? SpecimenSourceId { get; private set; }

    public Guid? ContainerTypeId { get; private set; }

    public decimal? RequiredQuantity { get; private set; }

    public string? QuantityUnitUcumCode { get; private set; }

    public string Notes { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public uint Version { get; private set; }

    public void UpdateQuantity(decimal? requiredQuantity, string? quantityUnitUcumCode)
    {
        SetQuantity(requiredQuantity, quantityUnitUcumCode);
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void UpdateNotes(string? notes)
    {
        Notes = CatalogCodes.RequireNotes(notes, nameof(notes));
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    private void SetQuantity(decimal? requiredQuantity, string? quantityUnitUcumCode)
    {
        if (requiredQuantity is null != (quantityUnitUcumCode is null))
        {
            throw new ArgumentException(
                "Required quantity and its UCUM unit must be provided together.",
                nameof(requiredQuantity));
        }

        if (requiredQuantity is < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(requiredQuantity),
                requiredQuantity,
                "Required quantity must not be negative.");
        }

        RequiredQuantity = requiredQuantity;
        QuantityUnitUcumCode = quantityUnitUcumCode is null
            ? null
            : CatalogCodes.RequireUcumCode(quantityUnitUcumCode, nameof(quantityUnitUcumCode));
    }
}
