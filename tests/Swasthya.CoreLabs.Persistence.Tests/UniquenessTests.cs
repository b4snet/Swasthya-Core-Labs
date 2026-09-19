using Microsoft.EntityFrameworkCore;
using Npgsql;
using Swasthya.CoreLabs.Domain.IdentityTenancy;
using Swasthya.CoreLabs.Infrastructure.Persistence;
using Swasthya.CoreLabs.TestSupport;

namespace Swasthya.CoreLabs.Persistence.Tests;

[Trait("Category", TestCategory.Db)]
public sealed class UniquenessTests : IClassFixture<PersistenceDbFixture>
{
    [Fact]
    public async Task Organization_DuplicateName_Throws()
    {
        using DbTestContextFactory factory = new(PersistenceDbFixture.ConnectionString);
        using CoreLabDbContext db = factory.Create();
        string name = $"dup-org-{Guid.NewGuid():N}";
        db.Organizations.Add(new Organization(name));
        await db.SaveChangesAsync();

        db.Organizations.Add(new Organization(name));

        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task Principal_DuplicateIdentity_Throws()
    {
        using DbTestContextFactory factory = new(PersistenceDbFixture.ConnectionString);
        using CoreLabDbContext db = factory.Create();
        string suffix = Guid.NewGuid().ToString("N");
        db.Principals.Add(new Principal(
            Domain.Common.PrincipalType.Human, "https://issuer.test", $"dup.{suffix}", "Dup"));
        await db.SaveChangesAsync();

        db.Principals.Add(new Principal(
            Domain.Common.PrincipalType.Human, "https://issuer.test", $"dup.{suffix}", "Dup 2"));

        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task Facility_DuplicateCodeWithinOrganization_Throws()
    {
        using DbTestContextFactory factory = new(PersistenceDbFixture.ConnectionString);
        using CoreLabDbContext db = factory.Create();
        var org = new Organization($"dup-org-{Guid.NewGuid():N}");
        db.Organizations.Add(org);
        await db.SaveChangesAsync();

        db.Facilities.Add(new Facility(org.Id, "DUP", "One"));
        await db.SaveChangesAsync();

        db.Facilities.Add(new Facility(org.Id, "DUP", "Two"));

        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task PrincipalRoleAssignment_DuplicateOrgScope_Throws()
    {
        using DbTestContextFactory factory = new(PersistenceDbFixture.ConnectionString);
        using CoreLabDbContext db = factory.Create();
        var seeded = SeedSamples.Seed(db, $"dup-pra-o-{Guid.NewGuid():N}");

        var principal = new Principal(
            Domain.Common.PrincipalType.Human,
            TestJwtDefaults.Issuer,
            $"dup-pra-o-{Guid.NewGuid():N}",
            "Dup");
        db.Principals.Add(principal);
        await db.SaveChangesAsync();

        var firstRole = new Role(seeded.OrganizationId, $"role-o-{Guid.NewGuid():N}", "One");
        var secondRole = new Role(seeded.OrganizationId, $"role-o-{Guid.NewGuid():N}", "Two");
        db.Roles.AddRange(firstRole, secondRole);
        await db.SaveChangesAsync();

        db.PrincipalRoleAssignments.Add(new PrincipalRoleAssignment(
            principal.Id, firstRole.Id, seeded.OrganizationId));
        await db.SaveChangesAsync();

        db.PrincipalRoleAssignments.Add(new PrincipalRoleAssignment(
            principal.Id, secondRole.Id, seeded.OrganizationId));

        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task PrincipalRoleAssignment_DuplicateFacilityScope_Throws()
    {
        using DbTestContextFactory factory = new(PersistenceDbFixture.ConnectionString);
        using CoreLabDbContext db = factory.Create();
        var seeded = SeedSamples.Seed(db, $"dup-pra-f-{Guid.NewGuid():N}");
        var freshPrincipal = new Principal(
            Domain.Common.PrincipalType.Human,
            TestJwtDefaults.Issuer,
            $"dup-pra-f-{Guid.NewGuid():N}",
            "Dup");
        db.Principals.Add(freshPrincipal);
        await db.SaveChangesAsync();

        var firstRole = new Role(seeded.OrganizationId, $"role-f-{Guid.NewGuid():N}", "One");
        var secondRole = new Role(seeded.OrganizationId, $"role-f-{Guid.NewGuid():N}", "Two");
        db.Roles.AddRange(firstRole, secondRole);
        await db.SaveChangesAsync();

        db.PrincipalRoleAssignments.Add(new PrincipalRoleAssignment(
            freshPrincipal.Id, firstRole.Id, seeded.OrganizationId, seeded.FacilityAId));
        await db.SaveChangesAsync();

        db.PrincipalRoleAssignments.Add(new PrincipalRoleAssignment(
            freshPrincipal.Id, secondRole.Id, seeded.OrganizationId, seeded.FacilityAId));

        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }
}
