using Swasthya.CoreLabs.Domain.Common;

namespace Swasthya.CoreLabs.Domain.IdentityTenancy;

public sealed class Organization
{
    private Organization()
    {
    }

    public Organization(string name)
    {
        Id = Guid.NewGuid();
        Name = !string.IsNullOrWhiteSpace(name)
            ? name
            : throw new ArgumentException("Organization name is required.", nameof(name));
        Status = EntityStatus.Active;
        CreatedAtUtc = DateTimeOffset.UtcNow;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public EntityStatus Status { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public uint Version { get; private set; }

    public IReadOnlyCollection<Facility> Facilities { get; private set; } = new List<Facility>();

    public IReadOnlyCollection<Role> Roles { get; private set; } = new List<Role>();

    public void Rename(string name)
    {
        Name = !string.IsNullOrWhiteSpace(name)
            ? name
            : throw new ArgumentException("Organization name is required.", nameof(name));
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void SetStatus(EntityStatus status)
    {
        Status = status;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }
}
