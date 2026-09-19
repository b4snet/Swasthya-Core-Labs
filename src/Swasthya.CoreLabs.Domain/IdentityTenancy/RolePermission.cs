using System.Diagnostics.CodeAnalysis;

namespace Swasthya.CoreLabs.Domain.IdentityTenancy;

[SuppressMessage("Design", "CA1711", Justification = "'RolePermission' is the intentional domain term for a role's access right.")]
public sealed class RolePermission
{
    private RolePermission()
    {
    }

    public RolePermission(Guid roleId, Guid permissionId)
    {
        Id = Guid.NewGuid();
        RoleId = roleId;
        PermissionId = permissionId;
        CreatedAtUtc = DateTimeOffset.UtcNow;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }

    public Guid RoleId { get; private set; }

    public Role? Role { get; private set; }

    public Guid PermissionId { get; private set; }

    public Permission? Permission { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public uint Version { get; private set; }
}
