namespace Swasthya.CoreLabs.Domain.Laboratory;

public sealed class Unit
{
    private Unit()
    {
    }

    public Unit(
        Guid organizationId,
        string code,
        string name,
        string ucumCode,
        string? description = null)
    {
        Id = Guid.NewGuid();
        OrganizationId = organizationId;
        Code = CatalogCodes.RequireCode(code, nameof(code));
        Name = CatalogCodes.RequireName(name, nameof(name));
        UcumCode = CatalogCodes.RequireUcumCode(ucumCode, nameof(ucumCode));
        Description = CatalogCodes.RequireDescription(description, nameof(description));
        Status = MasterDataStatus.Draft;
        CreatedAtUtc = DateTimeOffset.UtcNow;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }

    public Guid OrganizationId { get; private set; }

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Canonical UCUM expression. UCUM is the unit representation boundary;
    /// no canonicalization/validation engine is implemented in this phase.
    /// </summary>
    public string UcumCode { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public MasterDataStatus Status { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public uint Version { get; private set; }

    public void Update(string name, string ucumCode, string? description = null)
    {
        Name = CatalogCodes.RequireName(name, nameof(name));
        UcumCode = CatalogCodes.RequireUcumCode(ucumCode, nameof(ucumCode));
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
