using Swasthya.CoreLabs.Domain.Common;

namespace Swasthya.CoreLabs.Domain.IdentityTenancy;

public sealed class Facility
{
    private Facility()
    {
    }

    public Facility(Guid organizationId, string code, string name)
    {
        Id = Guid.NewGuid();
        OrganizationId = organizationId;
        Code = !string.IsNullOrWhiteSpace(code)
            ? code
            : throw new ArgumentException("Facility code is required.", nameof(code));
        Name = !string.IsNullOrWhiteSpace(name)
            ? name
            : throw new ArgumentException("Facility name is required.", nameof(name));
        Status = EntityStatus.Active;
        CreatedAtUtc = DateTimeOffset.UtcNow;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }

    public Guid OrganizationId { get; private set; }

    public Organization? Organization { get; private set; }

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public EntityStatus Status { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public uint Version { get; private set; }

    public void Rename(string name)
    {
        Name = !string.IsNullOrWhiteSpace(name)
            ? name
            : throw new ArgumentException("Facility name is required.", nameof(name));
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void SetStatus(EntityStatus status)
    {
        Status = status;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }
}
