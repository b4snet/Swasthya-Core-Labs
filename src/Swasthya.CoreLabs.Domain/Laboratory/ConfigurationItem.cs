namespace Swasthya.CoreLabs.Domain.Laboratory;

/// <summary>
/// Operational configuration entry. Organization-wide when
/// <see cref="FacilityId"/> is null, otherwise facility-specific. This is
/// operational configuration, not the deferred jurisdictional/clinical rules
/// registry.
/// </summary>
public sealed class ConfigurationItem
{
    private ConfigurationItem()
    {
    }

    public ConfigurationItem(
        Guid organizationId,
        Guid? facilityId,
        string key,
        string value,
        ConfigurationValueType valueType,
        string? description = null)
    {
        Id = Guid.NewGuid();
        OrganizationId = organizationId;
        FacilityId = facilityId;
        Key = CatalogCodes.RequireConfigurationKey(key, nameof(key));
        Value = value ?? string.Empty;
        ValueType = valueType;
        Description = CatalogCodes.RequireDescription(description, nameof(description));
        Status = MasterDataStatus.Draft;
        CreatedAtUtc = DateTimeOffset.UtcNow;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }

    public Guid OrganizationId { get; private set; }

    public Guid? FacilityId { get; private set; }

    public string Key { get; private set; } = string.Empty;

    public string Value { get; private set; } = string.Empty;

    public ConfigurationValueType ValueType { get; private set; }

    public string Description { get; private set; } = string.Empty;

    public MasterDataStatus Status { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public uint Version { get; private set; }

    public void Update(string value, ConfigurationValueType valueType, string? description = null)
    {
        Value = value ?? string.Empty;
        ValueType = valueType;
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
