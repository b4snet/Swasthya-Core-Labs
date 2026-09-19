namespace Swasthya.CoreLabs.Domain.Laboratory;

/// <summary>
/// Stable identity for a test-catalog entry. Clinically significant definition
/// fields live on immutable <see cref="TestVersion"/> rows; this row holds the
/// mutable lifecycle status and the current version pointer.
/// </summary>
public sealed class Test
{
    private Test()
    {
    }

    public Test(Guid organizationId, string code)
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

    /// <summary>
    /// Advances the current-version pointer and returns the new version number.
    /// Called when a new immutable <see cref="TestVersion"/> is appended.
    /// </summary>
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
