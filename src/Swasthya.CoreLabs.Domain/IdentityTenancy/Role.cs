using Swasthya.CoreLabs.Domain.Common;

namespace Swasthya.CoreLabs.Domain.IdentityTenancy;

public sealed class Role
{
    private Role()
    {
    }

    public Role(Guid organizationId, string code, string name, string? description = null)
    {
        Id = Guid.NewGuid();
        OrganizationId = organizationId;
        Code = !string.IsNullOrWhiteSpace(code)
            ? code
            : throw new ArgumentException("Role code is required.", nameof(code));
        Name = !string.IsNullOrWhiteSpace(name)
            ? name
            : throw new ArgumentException("Role name is required.", nameof(name));
        Description = description;
        Status = EntityStatus.Active;
        CreatedAtUtc = DateTimeOffset.UtcNow;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }

    public Guid OrganizationId { get; private set; }

    public Organization? Organization { get; private set; }

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public EntityStatus Status { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public uint Version { get; private set; }

    public IReadOnlyCollection<RolePermission> RolePermissions { get; private set; } = new List<RolePermission>();

    public void SetStatus(EntityStatus status)
    {
        Status = status;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }
}
