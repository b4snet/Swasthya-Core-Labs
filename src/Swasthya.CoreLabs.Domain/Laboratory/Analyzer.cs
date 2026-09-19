namespace Swasthya.CoreLabs.Domain.Laboratory;

/// <summary>
/// Facility-owned analyzer/device registry entry. Configuration/ownership only;
/// no analyzer communication, protocol adapter, or test mapping is implemented
/// in this phase.
/// </summary>
public sealed class Analyzer
{
    private Analyzer()
    {
    }

    public Analyzer(
        Guid organizationId,
        Guid facilityId,
        string code,
        string name,
        string? manufacturer = null,
        string? model = null,
        string? description = null)
    {
        Id = Guid.NewGuid();
        OrganizationId = organizationId;
        FacilityId = facilityId;
        Code = CatalogCodes.RequireCode(code, nameof(code));
        Name = CatalogCodes.RequireName(name, nameof(name));
        Manufacturer = (manufacturer ?? string.Empty).Trim();
        Model = (model ?? string.Empty).Trim();
        Description = CatalogCodes.RequireDescription(description, nameof(description));
        Status = MasterDataStatus.Draft;
        CreatedAtUtc = DateTimeOffset.UtcNow;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }

    public Guid OrganizationId { get; private set; }

    public Guid FacilityId { get; private set; }

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string Manufacturer { get; private set; } = string.Empty;

    public string Model { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public MasterDataStatus Status { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public uint Version { get; private set; }

    public void Update(
        string name,
        string? manufacturer = null,
        string? model = null,
        string? description = null)
    {
        Name = CatalogCodes.RequireName(name, nameof(name));
        Manufacturer = (manufacturer ?? string.Empty).Trim();
        Model = (model ?? string.Empty).Trim();
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
