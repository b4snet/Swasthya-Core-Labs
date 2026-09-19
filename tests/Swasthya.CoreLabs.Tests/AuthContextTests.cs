using Swasthya.CoreLabs.Application.Authorization;
using Swasthya.CoreLabs.Application.Exceptions;
using Swasthya.CoreLabs.Application.Models;
using Swasthya.CoreLabs.Domain.Common;

namespace Swasthya.CoreLabs.Tests;

public class AuthContextTests
{
    private static readonly Guid Org = Guid.NewGuid();
    private static readonly Guid FacilityA = Guid.NewGuid();
    private static readonly Guid FacilityB = Guid.NewGuid();

    private static AuthContext ContextWithFacilityGrants(params Guid[] facilities) =>
        AuthContext.Resolved(
            Guid.NewGuid(),
            "https://issuer.test",
            "sub-1",
            PrincipalType.Human,
            facilities
                .Select(f => new AuthContextGrant("x.read", Org, f))
                .ToArray());

    private static AuthContext ContextWithOrgGrants(params string[] codes) =>
        AuthContext.Resolved(
            Guid.NewGuid(),
            "https://issuer.test",
            "sub-1",
            PrincipalType.Human,
            codes.Select(c => new AuthContextGrant(c, Org, null)).ToArray());

    [Fact]
    public void OrgWideGrant_SatisfiesFacilityGuard()
    {
        AuthContext context = ContextWithOrgGrants("x.read");

        PermissionGuard.Require(context, "x.read", Org, FacilityA);
    }

    [Fact]
    public void FacilityGrant_SatisfiesItsOwnFacility()
    {
        AuthContext context = ContextWithFacilityGrants(FacilityA);

        PermissionGuard.Require(context, "x.read", Org, FacilityA);
    }

    [Fact]
    public void OtherOrganization_DoesNotSatisfyGuard()
    {
        AuthContext context = ContextWithFacilityGrants(FacilityA);

        Assert.Throws<PermissionDeniedException>(
            () => PermissionGuard.Require(context, "x.read", Guid.NewGuid(), FacilityA));
    }

    [Fact]
    public void FacilityGrant_SatisfiesOrganizationLevelGuard()
    {
        AuthContext context = ContextWithFacilityGrants(FacilityA);

        PermissionGuard.Require(context, "x.read", Org);
    }

    [Fact]
    public void MissingGrant_ThrowsPermissionDenied()
    {
        AuthContext context = ContextWithOrgGrants("y.read");

        Assert.Throws<PermissionDeniedException>(
            () => PermissionGuard.Require(context, "x.read", Org));
    }

    [Fact]
    public void OrganizationIds_AreDistinct()
    {
        AuthContext context = AuthContext.Resolved(
            Guid.NewGuid(),
            "https://issuer.test",
            "sub-1",
            PrincipalType.Human,
            new[]
            {
                new AuthContextGrant("x.read", Org, FacilityA),
                new AuthContextGrant("x.write", Org, FacilityB),
                new AuthContextGrant("x.read", Guid.NewGuid(), null),
            });

        Assert.Equal(2, context.OrganizationIds.Count);
    }

    [Fact]
    public void Denied_IsNeverAuthenticated()
    {
        Assert.False(AuthContext.Denied.IsAuthenticated);
    }
}
