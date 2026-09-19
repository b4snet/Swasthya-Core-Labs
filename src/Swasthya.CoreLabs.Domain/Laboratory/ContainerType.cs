namespace Swasthya.CoreLabs.Domain.Laboratory;

public sealed class ContainerType
{
    private ContainerType()
    {
    }

    public ContainerType(
        Guid organizationId,
        string code,
        string name,
        string? description = null,
        string? handlingNotes = null)
    {
        Id = Guid.NewGuid();
        OrganizationId = organizationId;
        Code = CatalogCodes.RequireCode(code, nameof(code));
        Name = CatalogCodes.RequireName(name, nameof(name));
        Description = CatalogCodes.RequireDescription(description, nameof(description));
        HandlingNotes = CatalogCodes.RequireNotes(handlingNotes, nameof(handlingNotes));
        Status = MasterDataStatus.Draft;
        CreatedAtUtc = DateTimeOffset.UtcNow;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }

    public Guid OrganizationId { get; private set; }

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public string HandlingNotes { get; private set; } = string.Empty;

    public MasterDataStatus Status { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public uint Version { get; private set; }

    public void Update(string name, string? description = null, string? handlingNotes = null)
    {
        Name = CatalogCodes.RequireName(name, nameof(name));
        Description = CatalogCodes.RequireDescription(description, nameof(description));
        HandlingNotes = CatalogCodes.RequireNotes(handlingNotes, nameof(handlingNotes));
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void SetStatus(MasterDataStatus status)
    {
        MasterDataLifecycle.RequireTransition(Status, status);
        Status = status;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }
}
