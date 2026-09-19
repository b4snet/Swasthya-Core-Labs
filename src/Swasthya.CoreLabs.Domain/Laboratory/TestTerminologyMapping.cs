namespace Swasthya.CoreLabs.Domain.Laboratory;

/// <summary>
/// Explicit mapping from an internal test concept to an external terminology
/// code. Only the mapping structure is stored; no proprietary terminology
/// content is embedded in the repository.
/// </summary>
public sealed class TestTerminologyMapping
{
    private TestTerminologyMapping()
    {
    }

    public TestTerminologyMapping(
        Guid testId,
        Guid codeSystemId,
        string externalCode,
        string? externalVersion = null,
        string? notes = null,
        DateTimeOffset? effectiveFromUtc = null,
        DateTimeOffset? effectiveToUtc = null)
    {
        Id = Guid.NewGuid();
        Guid codeSystem = codeSystemId != Guid.Empty
            ? codeSystemId
            : throw new ArgumentException("A code system is required.", nameof(codeSystemId));

        DateTimeOffset from = effectiveFromUtc ?? DateTimeOffset.UtcNow;
        if (effectiveToUtc is { } to && to < from)
        {
            throw new ArgumentException(
                "Effective-to must not precede effective-from.",
                nameof(effectiveToUtc));
        }

        TestId = testId;
        CodeSystemId = codeSystem;
        ExternalCode = CatalogCodes.RequireExternalCode(externalCode, nameof(externalCode));
        ExternalVersion = (externalVersion ?? string.Empty).Trim();
        Notes = CatalogCodes.RequireNotes(notes, nameof(notes));
        Status = TerminologyMappingStatus.Proposed;
        EffectiveFromUtc = from;
        EffectiveToUtc = effectiveToUtc;
        CreatedAtUtc = DateTimeOffset.UtcNow;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }

    public Guid TestId { get; private set; }

    public Guid CodeSystemId { get; private set; }

    public string ExternalCode { get; private set; } = string.Empty;

    public string ExternalVersion { get; private set; } = string.Empty;

    public TerminologyMappingStatus Status { get; private set; }

    public string Notes { get; private set; } = string.Empty;

    public DateTimeOffset EffectiveFromUtc { get; private set; }

    public DateTimeOffset? EffectiveToUtc { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public uint Version { get; private set; }

    public void SetStatus(TerminologyMappingStatus status)
    {
        if (Status == status)
        {
            throw new InvalidOperationException(
                $"Terminology mapping is already '{status}'.");
        }

        bool allowed = (Status, status) switch
        {
            (TerminologyMappingStatus.Proposed, TerminologyMappingStatus.Active) => true,
            (TerminologyMappingStatus.Proposed, TerminologyMappingStatus.Superseded) => true,
            (TerminologyMappingStatus.Active, TerminologyMappingStatus.Superseded) => true,
            _ => false,
        };

        if (!allowed)
        {
            throw new InvalidOperationException(
                $"Terminology mapping cannot transition from '{Status}' to '{status}'.");
        }

        Status = status;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }
}
