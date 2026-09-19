using Swasthya.CoreLabs.Domain.Common;

namespace Swasthya.CoreLabs.Application.Models;

public sealed record AuthContextGrant(string PermissionCode, Guid OrganizationId, Guid? FacilityId);

public sealed class AuthContext
{
    public static AuthContext Denied { get; } = new(
        Guid.Empty, string.Empty, string.Empty, PrincipalType.Human, false,
        Array.Empty<AuthContextGrant>());

    private AuthContext(
        Guid principalId,
        string issuer,
        string externalSubject,
        PrincipalType principalType,
        bool isAuthenticated,
        IReadOnlyCollection<AuthContextGrant> grants)
    {
        PrincipalId = principalId;
        Issuer = issuer;
        ExternalSubject = externalSubject;
        PrincipalType = principalType;
        IsAuthenticated = isAuthenticated;
        Grants = grants;
    }

    public Guid PrincipalId { get; }

    public string Issuer { get; }

    public string ExternalSubject { get; }

    public PrincipalType PrincipalType { get; }

    public bool IsAuthenticated { get; }

    public IReadOnlyCollection<AuthContextGrant> Grants { get; }

    public static AuthContext Resolved(
        Guid principalId,
        string issuer,
        string externalSubject,
        PrincipalType principalType,
        IReadOnlyCollection<AuthContextGrant> grants) =>
        new(principalId, issuer, externalSubject, principalType, true,
            grants ?? Array.Empty<AuthContextGrant>());

    public bool IsGranted(string permissionCode, Guid? organizationId = null, Guid? facilityId = null) =>
        Grants.Any(g =>
            string.Equals(g.PermissionCode, permissionCode, StringComparison.Ordinal)
            && (organizationId is null || g.OrganizationId == organizationId)
            && (facilityId is null || g.FacilityId == facilityId));

    public bool IsGrantedAtOrg(string permissionCode, Guid organizationId) =>
        Grants.Any(g =>
            string.Equals(g.PermissionCode, permissionCode, StringComparison.Ordinal)
            && g.OrganizationId == organizationId);

    public IReadOnlyCollection<Guid> OrganizationIds =>
        Grants.Select(g => g.OrganizationId).Distinct().ToArray();
}
