namespace Swasthya.CoreLabs.Domain.Laboratory;

/// <summary>
/// Immutable, append-only snapshot of a test definition. Never updated once
/// persisted; a change to any definition field publishes a new version.
/// </summary>
public sealed class TestVersion
{
    private TestVersion()
    {
    }

    public TestVersion(
        Guid testId,
        int versionNumber,
        string name,
        Guid resultDataTypeId,
        Guid? disciplineId = null,
        Guid? unitId = null,
        string? description = null,
        DateTimeOffset? effectiveFromUtc = null,
        DateTimeOffset? effectiveToUtc = null)
    {
        Id = Guid.NewGuid();
        if (versionNumber < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(versionNumber),
                versionNumber,
                "Version number must be at least 1.");
        }

        Guid resultDataType = resultDataTypeId != Guid.Empty
            ? resultDataTypeId
            : throw new ArgumentException("A result data type is required.", nameof(resultDataTypeId));

        DateTimeOffset from = effectiveFromUtc ?? DateTimeOffset.UtcNow;
        if (effectiveToUtc is { } to && to < from)
        {
            throw new ArgumentException(
                "Effective-to must not precede effective-from.",
                nameof(effectiveToUtc));
        }

        TestId = testId;
        VersionNumber = versionNumber;
        Name = CatalogCodes.RequireName(name, nameof(name));
        Description = CatalogCodes.RequireDescription(description, nameof(description));
        DisciplineId = disciplineId;
        ResultDataTypeId = resultDataType;
        UnitId = unitId;
        EffectiveFromUtc = from;
        EffectiveToUtc = effectiveToUtc;
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }

    public Guid TestId { get; private set; }

    public int VersionNumber { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public Guid? DisciplineId { get; private set; }

    public Guid ResultDataTypeId { get; private set; }

    public Guid? UnitId { get; private set; }

    public DateTimeOffset EffectiveFromUtc { get; private set; }

    public DateTimeOffset? EffectiveToUtc { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }
}
