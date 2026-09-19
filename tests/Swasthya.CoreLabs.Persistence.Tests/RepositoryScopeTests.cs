using Swasthya.CoreLabs.Application.Abstractions;
using Swasthya.CoreLabs.Infrastructure.Persistence;
using Swasthya.CoreLabs.TestSupport;

namespace Swasthya.CoreLabs.Persistence.Tests;

[Trait("Category", TestCategory.Db)]
public sealed class RepositoryScopeTests : IClassFixture<PersistenceDbFixture>
{
    [Fact]
    public async Task LoadGrants_Admin_HasOrgWideGrants()
    {
        using DbTestContextFactory factory = NewFactory();
        using CoreLabDbContext db = factory.Create();
        SeededTenant tenant = SeedSamples.Seed(db, $"repo-admin-{Guid.NewGuid():N}");
        var repository = new AuthorizationRepository(CreateContext());

        var grants = await repository.LoadGrantsAsync(tenant.AdminPrincipalId, CancellationToken.None);

        Assert.Contains(
            grants,
            g => g.PermissionCode == "organization.list"
                && g.OrganizationId == tenant.OrganizationId
                && g.FacilityId is null);
        Assert.Contains(
            grants,
            g => g.PermissionCode == "facility.list"
                && g.OrganizationId == tenant.OrganizationId
                && g.FacilityId is null);
        Assert.Contains(
            grants,
            g => g.PermissionCode == "audit.read"
                && g.OrganizationId == tenant.OrganizationId
                && g.FacilityId is null);
    }

    [Fact]
    public async Task LoadGrants_Staff_HasOnlyFacilityScopedGrants()
    {
        using DbTestContextFactory factory = NewFactory();
        using CoreLabDbContext db = factory.Create();
        SeededTenant tenant = SeedSamples.Seed(db, $"repo-staff-{Guid.NewGuid():N}");
        var repository = new AuthorizationRepository(CreateContext());

        var grants = await repository.LoadGrantsAsync(tenant.StaffPrincipalId, CancellationToken.None);

        Assert.Equal(3, grants.Count);
        Assert.All(grants, g =>
        {
            Assert.Equal(tenant.OrganizationId, g.OrganizationId);
            Assert.Equal(tenant.FacilityAId, g.FacilityId);
        });
        Assert.Contains(grants, g => g.PermissionCode == "organization.list");
        Assert.Contains(grants, g => g.PermissionCode == "facility.list");
        Assert.Contains(grants, g => g.PermissionCode == "audit.read");
    }

    [Fact]
    public async Task LoadGrants_InactiveOrganization_ReturnsNone()
    {
        using DbTestContextFactory factory = NewFactory();
        using CoreLabDbContext db = factory.Create();
        SeededTenant tenant = SeedSamples.Seed(db, $"repo-inactive-{Guid.NewGuid():N}");
        var organization = db.Organizations.First(o => o.Id == tenant.OrganizationId);
        organization.SetStatus(Domain.Common.EntityStatus.Inactive);
        await db.SaveChangesAsync();
        var repository = new AuthorizationRepository(CreateContext());

        var grants = await repository.LoadGrantsAsync(tenant.StaffPrincipalId, CancellationToken.None);

        Assert.Empty(grants);
    }

    [Fact]
    public async Task ListAudit_FacilityRestricted_SeesOnlyThatFacility()
    {
        using DbTestContextFactory factory = NewFactory();
        using CoreLabDbContext db = factory.Create();
        SeededTenant tenant = SeedSamples.Seed(db, $"aud-fac-{Guid.NewGuid():N}");
        SeedSamples.AddAuditRecord(
            db, tenant.OrganizationId, tenant.FacilityAId, tenant.AdminPrincipalId,
            null, "seed.record.fa", $"corr-{Guid.NewGuid():N}");
        SeedSamples.AddAuditRecord(
            db, tenant.OrganizationId, tenant.FacilityBId, tenant.AdminPrincipalId,
            null, "seed.record.fb", $"corr-{Guid.NewGuid():N}");
        var repository = new AuditRepository(CreateContext());

        var records = await repository.ListAsync(
            Array.Empty<Guid>(), [tenant.FacilityAId], 0, 50, CancellationToken.None);

        Assert.Contains(records, r => r.Action == "seed.record.fa");
        Assert.DoesNotContain(records, r => r.Action == "seed.record.fb");
    }

    [Fact]
    public async Task ListAudit_OrgWide_SeesAllFacilities()
    {
        using DbTestContextFactory factory = NewFactory();
        using CoreLabDbContext db = factory.Create();
        SeededTenant tenant = SeedSamples.Seed(db, $"aud-org-{Guid.NewGuid():N}");
        SeedSamples.AddAuditRecord(
            db, tenant.OrganizationId, tenant.FacilityAId, tenant.AdminPrincipalId,
            null, "seed.record.fa", $"corr-{Guid.NewGuid():N}");
        SeedSamples.AddAuditRecord(
            db, tenant.OrganizationId, tenant.FacilityBId, tenant.AdminPrincipalId,
            null, "seed.record.fb", $"corr-{Guid.NewGuid():N}");
        var repository = new AuditRepository(CreateContext());

        var records = await repository.ListAsync(
            [tenant.OrganizationId], null, 0, 50, CancellationToken.None);

        Assert.Contains(records, r => r.Action == "seed.record.fa");
        Assert.Contains(records, r => r.Action == "seed.record.fb");
    }

    [Fact]
    public async Task ListOrgByIds_EmptySet_ReturnsEmpty()
    {
        var repository = new OrganizationRepository(CreateContext());

        var organizations = await repository.ListByIdsAsync(
            Array.Empty<Guid>(), CancellationToken.None);

        Assert.Empty(organizations);
    }

    private static DbTestContextFactory NewFactory() =>
        new(PersistenceDbFixture.ConnectionString);

    private static CoreLabDbContext CreateContext() =>
        new DbTestContextFactory(PersistenceDbFixture.ConnectionString).Create();
}
