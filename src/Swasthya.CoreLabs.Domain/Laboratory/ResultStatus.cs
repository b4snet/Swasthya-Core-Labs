namespace Swasthya.CoreLabs.Domain.Laboratory;

/// <summary>
/// Configurable result/test status metadata. This phase stores the catalog only;
/// no result-state machine or interpretation logic is implemented.
/// </summary>
public sealed class ResultStatus
{
    private ResultStatus()
    {
    }

    public ResultStatus(Guid organizationId, string code, string name, string? description = null)
    {
        Id = Guid.NewGuid();
        OrganizationId = organizationId;
        Code = CatalogCodes.RequireCode(code, nameof(code));
        Name = CatalogCodes.RequireName(name, nameof(name));
        Description = CatalogCodes.RequireDescription(description, nameof(description));
        Status = MasterDataStatus.Draft;
        CreatedAtUtc = DateTimeOffset.UtcNow;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }

    public Guid OrganizationId { get; private set; }

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public MasterDataStatus Status { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public uint Version { get; private set; }

    public void Update(string name, string? description = null)
    {
        Name = CatalogCodes.RequireName(name, nameof(name));
        Description = CatalogCodes.RequireDescription(description, nameof(description));
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void SetStatus(MasterDataStatus status)
    {
        MasterDataLifecycle.RequireTransition(Status, status);
        Status = status;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }
}
