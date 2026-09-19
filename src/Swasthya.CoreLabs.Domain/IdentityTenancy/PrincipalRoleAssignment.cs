using Swasthya.CoreLabs.Domain.Common;

namespace Swasthya.CoreLabs.Domain.IdentityTenancy;

public sealed class PrincipalRoleAssignment
{
    private PrincipalRoleAssignment()
    {
    }

    public PrincipalRoleAssignment(
        Guid principalId,
        Guid roleId,
        Guid organizationId,
        Guid? facilityId = null)
    {
        Id = Guid.NewGuid();
        PrincipalId = principalId;
        RoleId = roleId;
        OrganizationId = organizationId;
        FacilityId = facilityId;
        CreatedAtUtc = DateTimeOffset.UtcNow;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }

    public Guid PrincipalId { get; private set; }

    public Principal? Principal { get; private set; }

    public Guid RoleId { get; private set; }

    public Role? Role { get; private set; }

    public Guid OrganizationId { get; private set; }

    public Organization? Organization { get; private set; }

    public Guid? FacilityId { get; private set; }

    public Facility? Facility { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public uint Version { get; private set; }
}
