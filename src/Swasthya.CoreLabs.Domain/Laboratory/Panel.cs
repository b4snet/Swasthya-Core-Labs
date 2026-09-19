namespace Swasthya.CoreLabs.Domain.Laboratory;

/// <summary>
/// Stable identity for a panel/profile. Clinically significant definition
/// fields live on immutable <see cref="PanelVersion"/> rows.
/// </summary>
public sealed class Panel
{
    private Panel()
    {
    }

    public Panel(Guid organizationId, string code)
    {
        Id = Guid.NewGuid();
        OrganizationId = organizationId;
        Code = CatalogCodes.RequireCode(code, nameof(code));
        Status = MasterDataStatus.Draft;
        CurrentVersionNumber = 0;
        CreatedAtUtc = DateTimeOffset.UtcNow;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }

    public Guid OrganizationId { get; private set; }

    public string Code { get; private set; } = string.Empty;

    public MasterDataStatus Status { get; private set; }

    public int CurrentVersionNumber { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public uint Version { get; private set; }

    public int StartNextVersion()
    {
        CurrentVersionNumber++;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
        return CurrentVersionNumber;
    }

    public void SetStatus(MasterDataStatus status)
    {
        MasterDataLifecycle.RequireTransition(Status, status);
        Status = status;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }
}

/// <summary>
/// Immutable, append-only snapshot of a panel definition.
/// </summary>
public sealed class PanelVersion
{
    private PanelVersion()
    {
    }

    public PanelVersion(
        Guid panelId,
        int versionNumber,
        string name,
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

        DateTimeOffset from = effectiveFromUtc ?? DateTimeOffset.UtcNow;
        if (effectiveToUtc is { } to && to < from)
        {
            throw new ArgumentException(
                "Effective-to must not precede effective-from.",
                nameof(effectiveToUtc));
        }

        PanelId = panelId;
        VersionNumber = versionNumber;
        Name = CatalogCodes.RequireName(name, nameof(name));
        Description = CatalogCodes.RequireDescription(description, nameof(description));
        EffectiveFromUtc = from;
        EffectiveToUtc = effectiveToUtc;
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }

    public Guid PanelId { get; private set; }

    public int VersionNumber { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public DateTimeOffset EffectiveFromUtc { get; private set; }

    public DateTimeOffset? EffectiveToUtc { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }
}

/// <summary>
/// Membership of a test concept in a panel. A test is referenced, never
/// duplicated, so one test may belong to many panels.
/// </summary>
public sealed class PanelTestMembership
{
    private PanelTestMembership()
    {
    }

    public PanelTestMembership(Guid panelId, Guid testId, int sequenceNumber, string? displayText = null)
    {
        Id = Guid.NewGuid();
        if (sequenceNumber < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(sequenceNumber),
                sequenceNumber,
                "Sequence number must not be negative.");
        }

        PanelId = panelId;
        TestId = testId;
        SequenceNumber = sequenceNumber;
        DisplayText = (displayText ?? string.Empty).Trim();
        CreatedAtUtc = DateTimeOffset.UtcNow;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }

    public Guid PanelId { get; private set; }

    public Guid TestId { get; private set; }

    public int SequenceNumber { get; private set; }

    public string DisplayText { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public uint Version { get; private set; }

    public void SetSequence(int sequenceNumber)
    {
        if (sequenceNumber < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(sequenceNumber),
                sequenceNumber,
                "Sequence number must not be negative.");
        }

        SequenceNumber = sequenceNumber;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void SetDisplayText(string? displayText)
    {
        DisplayText = (displayText ?? string.Empty).Trim();
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }
}
