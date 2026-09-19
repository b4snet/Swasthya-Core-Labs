namespace Swasthya.CoreLabs.Domain.Laboratory;

/// <summary>
/// Configurable reference interval for a test concept. This phase stores the
/// structure only: no clinical ranges are populated and no interpretation or
/// abnormality logic is implemented.
/// </summary>
public sealed class ReferenceRange
{
    private ReferenceRange()
    {
    }

    public ReferenceRange(
        Guid organizationId,
        Guid? facilityId,
        Guid testId,
        string source,
        string? population = null,
        ReferenceRangeSex? sex = null,
        decimal? ageMinimum = null,
        decimal? ageMaximum = null,
        string? ageUnitUcumCode = null,
        string? specimenContext = null,
        decimal? lowValue = null,
        decimal? highValue = null,
        string? textualReference = null,
        Guid? unitId = null,
        DateTimeOffset? effectiveFromUtc = null,
        DateTimeOffset? effectiveToUtc = null)
    {
        Id = Guid.NewGuid();
        if (string.IsNullOrWhiteSpace(source))
        {
            throw new ArgumentException("A provenance/source is required.", nameof(source));
        }

        source = source.Trim();
        if (source.Length > 300)
        {
            throw new ArgumentException("Source must not exceed 300 characters.", nameof(source));
        }

        if (ageMinimum is not null || ageMaximum is not null)
        {
            if (string.IsNullOrWhiteSpace(ageUnitUcumCode))
            {
                throw new ArgumentException(
                    "An age unit is required when age bounds are provided.",
                    nameof(ageUnitUcumCode));
            }

            if (ageMinimum is < 0 || ageMaximum is < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(ageMinimum),
                    "Age bounds must not be negative.");
            }

            if (ageMinimum is { } min && ageMaximum is { } max && min > max)
            {
                throw new ArgumentException(
                    "Age minimum must not exceed age maximum.",
                    nameof(ageMinimum));
            }
        }

        if ((lowValue is null) != (highValue is null))
        {
            throw new ArgumentException(
                "Quantitative reference bounds require both a low and a high value.",
                nameof(lowValue));
        }

        if (lowValue is { } low && highValue is { } high && low > high)
        {
            throw new ArgumentException(
                "Reference low must not exceed reference high.",
                nameof(lowValue));
        }

        bool hasQuantitative = lowValue is not null && highValue is not null;
        bool hasTextual = !string.IsNullOrWhiteSpace(textualReference);
        if (!hasQuantitative && !hasTextual)
        {
            throw new ArgumentException(
                "A reference range must provide quantitative bounds or a textual description.");
        }

        DateTimeOffset from = effectiveFromUtc ?? DateTimeOffset.UtcNow;
        if (effectiveToUtc is { } to && to < from)
        {
            throw new ArgumentException(
                "Effective-to must not precede effective-from.",
                nameof(effectiveToUtc));
        }

        OrganizationId = organizationId;
        FacilityId = facilityId;
        TestId = testId;
        Source = source;
        Population = TrimTo(population, 200, nameof(population));
        Sex = sex;
        AgeMinimum = ageMinimum;
        AgeMaximum = ageMaximum;
        AgeUnitUcumCode = string.IsNullOrWhiteSpace(ageUnitUcumCode)
            ? null
            : CatalogCodes.RequireUcumCode(ageUnitUcumCode, nameof(ageUnitUcumCode));
        SpecimenContext = TrimTo(specimenContext, 200, nameof(specimenContext));
        LowValue = lowValue;
        HighValue = highValue;
        TextualReference = TrimTo(textualReference, 500, nameof(textualReference));
        UnitId = unitId;
        EffectiveFromUtc = from;
        EffectiveToUtc = effectiveToUtc;
        Status = MasterDataStatus.Draft;
        CreatedAtUtc = DateTimeOffset.UtcNow;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }

    public Guid OrganizationId { get; private set; }

    public Guid? FacilityId { get; private set; }

    public Guid TestId { get; private set; }

    public string Source { get; private set; } = string.Empty;

    public string Population { get; private set; } = string.Empty;

    public ReferenceRangeSex? Sex { get; private set; }

    public decimal? AgeMinimum { get; private set; }

    public decimal? AgeMaximum { get; private set; }

    public string? AgeUnitUcumCode { get; private set; }

    public string SpecimenContext { get; private set; } = string.Empty;

    public decimal? LowValue { get; private set; }

    public decimal? HighValue { get; private set; }

    public string TextualReference { get; private set; } = string.Empty;

    public Guid? UnitId { get; private set; }

    public DateTimeOffset EffectiveFromUtc { get; private set; }

    public DateTimeOffset? EffectiveToUtc { get; private set; }

    public MasterDataStatus Status { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public uint Version { get; private set; }

    public void SetStatus(MasterDataStatus status)
    {
        MasterDataLifecycle.RequireTransition(Status, status);
        Status = status;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    private static string TrimTo(string? value, int maxLength, string paramName)
    {
        value = (value ?? string.Empty).Trim();
        if (value.Length > maxLength)
        {
            throw new ArgumentException(
                $"Value must not exceed {maxLength} characters.",
                paramName);
        }

        return value;
    }
}
