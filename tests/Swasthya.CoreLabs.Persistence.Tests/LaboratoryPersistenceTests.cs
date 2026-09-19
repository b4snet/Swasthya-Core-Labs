using Microsoft.EntityFrameworkCore;
using Swasthya.CoreLabs.Domain.Laboratory;
using Swasthya.CoreLabs.Infrastructure.Persistence;
using Swasthya.CoreLabs.TestSupport;

namespace Swasthya.CoreLabs.Persistence.Tests;

[Trait("Category", TestCategory.Db)]
public sealed class LaboratoryPersistenceTests : IClassFixture<PersistenceDbFixture>
{
    [Fact]
    public async Task SeededMasterData_RoundTripsAndIsOrganizationScoped()
    {
        using DbTestContextFactory factory = new(PersistenceDbFixture.ConnectionString);
        using CoreLabDbContext db = factory.Create();
        SeededTenant first = SeedSamples.Seed(db, $"lab-a-{Guid.NewGuid():N}");
        SeededTenant second = SeedSamples.Seed(db, $"lab-b-{Guid.NewGuid():N}");
        SeededLaboratory firstLab = SeedSamples.SeedLaboratory(db, first);
        SeedSamples.SeedLaboratory(db, second);

        List<Test> tests = await db.Tests.AsNoTracking()
            .Where(t => t.OrganizationId == first.OrganizationId)
            .ToListAsync();

        Test test = Assert.Single(tests);
        Assert.Equal(firstLab.TestCode, test.Code);
        Assert.Equal(1, test.CurrentVersionNumber);

        List<TestVersion> versions = await db.TestVersions.AsNoTracking()
            .Where(v => v.TestId == test.Id)
            .ToListAsync();
        TestVersion version = Assert.Single(versions);
        Assert.Equal(firstLab.ResultDataTypeId, version.ResultDataTypeId);
        Assert.Equal(firstLab.DisciplineId, version.DisciplineId);
        Assert.Equal(firstLab.UnitId, version.UnitId);
    }

    [Fact]
    public async Task Test_DuplicateCodeWithinOrganization_Throws()
    {
        using DbTestContextFactory factory = new(PersistenceDbFixture.ConnectionString);
        using CoreLabDbContext db = factory.Create();
        SeededTenant tenant = SeedSamples.Seed(db, $"lab-dup-test-{Guid.NewGuid():N}");
        SeededLaboratory lab = SeedSamples.SeedLaboratory(db, tenant);

        db.Tests.Add(new Test(tenant.OrganizationId, lab.TestCode));

        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task ConfigurationItem_DuplicateKeyInSameScope_ThrowsButOtherScopeAllowed()
    {
        using DbTestContextFactory factory = new(PersistenceDbFixture.ConnectionString);
        using CoreLabDbContext db = factory.Create();
        SeededTenant tenant = SeedSamples.Seed(db, $"lab-dup-cfg-{Guid.NewGuid():N}");
        string key = $"lab.{Guid.NewGuid():N}.flag";

        db.ConfigurationItems.Add(new ConfigurationItem(
            tenant.OrganizationId, null, key, "true", ConfigurationValueType.Boolean));
        db.ConfigurationItems.Add(new ConfigurationItem(
            tenant.OrganizationId, tenant.FacilityAId, key, "false", ConfigurationValueType.Boolean));
        await db.SaveChangesAsync();

        db.ConfigurationItems.Add(new ConfigurationItem(
            tenant.OrganizationId, null, key, "false", ConfigurationValueType.Boolean));

        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task PanelMembership_ReusesTestConceptWithoutDuplication()
    {
        using DbTestContextFactory factory = new(PersistenceDbFixture.ConnectionString);
        using CoreLabDbContext db = factory.Create();
        SeededTenant tenant = SeedSamples.Seed(db, $"lab-panel-{Guid.NewGuid():N}");
        SeededLaboratory lab = SeedSamples.SeedLaboratory(db, tenant);

        var panel = new Panel(tenant.OrganizationId, $"panel_{Guid.NewGuid():N}");
        int versionNumber = panel.StartNextVersion();
        db.Panels.Add(panel);
        db.PanelVersions.Add(new PanelVersion(panel.Id, versionNumber, "Panel"));
        db.PanelTestMemberships.Add(new PanelTestMembership(panel.Id, lab.TestId, 1));
        await db.SaveChangesAsync();

        db.PanelTestMemberships.Add(new PanelTestMembership(panel.Id, lab.TestId, 2));

        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task TestSpecimenRequirement_DuplicateSpecimenTypePerTest_Throws()
    {
        using DbTestContextFactory factory = new(PersistenceDbFixture.ConnectionString);
        using CoreLabDbContext db = factory.Create();
        SeededTenant tenant = SeedSamples.Seed(db, $"lab-spec-{Guid.NewGuid():N}");
        SeededLaboratory lab = SeedSamples.SeedLaboratory(db, tenant);

        db.TestSpecimenRequirements.Add(new TestSpecimenRequirement(
            lab.TestId, lab.SpecimenTypeId));
        await db.SaveChangesAsync();

        db.TestSpecimenRequirements.Add(new TestSpecimenRequirement(
            lab.TestId, lab.SpecimenTypeId));

        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task ConcurrentLifecycleMutation_IsRejected()
    {
        using DbTestContextFactory factory = new(PersistenceDbFixture.ConnectionString);
        SeededTenant tenant;
        Guid testId;
        using (CoreLabDbContext seed = factory.Create())
        {
            tenant = SeedSamples.Seed(seed, $"lab-conc-{Guid.NewGuid():N}");
            testId = SeedSamples.SeedLaboratory(seed, tenant).TestId;
        }

        using CoreLabDbContext first = factory.Create();
        using CoreLabDbContext second = factory.Create();
        Test firstTest = await first.Tests.SingleAsync(t => t.Id == testId);
        Test secondTest = await second.Tests.SingleAsync(t => t.Id == testId);

        firstTest.StartNextVersion();
        await first.SaveChangesAsync();

        secondTest.StartNextVersion();

        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => second.SaveChangesAsync());
    }

    [Fact]
    public async Task ReferentialConstraint_UnknownTestVersionTarget_IsRejected()
    {
        using DbTestContextFactory factory = new(PersistenceDbFixture.ConnectionString);
        using CoreLabDbContext db = factory.Create();
        SeededTenant tenant = SeedSamples.Seed(db, $"lab-fk-{Guid.NewGuid():N}");

        db.TestVersions.Add(new TestVersion(
            Guid.NewGuid(), 1, "Orphan", Guid.NewGuid()));

        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task ReferenceRange_StructurePersistsWithSourceAndEffectiveWindow()
    {
        using DbTestContextFactory factory = new(PersistenceDbFixture.ConnectionString);
        using CoreLabDbContext db = factory.Create();
        SeededTenant tenant = SeedSamples.Seed(db, $"lab-rr-{Guid.NewGuid():N}");
        SeededLaboratory lab = SeedSamples.SeedLaboratory(db, tenant);
        DateTimeOffset from = DateTimeOffset.UtcNow;
        from = new DateTimeOffset(from.Ticks - (from.Ticks % 10), from.Offset);

        var range = new ReferenceRange(
            tenant.OrganizationId,
            tenant.FacilityAId,
            lab.TestId,
            "provider verified worksheet",
            population: "adult",
            sex: ReferenceRangeSex.Any,
            lowValue: 3m,
            highValue: 7m,
            unitId: lab.UnitId,
            effectiveFromUtc: from);
        db.ReferenceRanges.Add(range);
        await db.SaveChangesAsync();

        ReferenceRange stored = await db.ReferenceRanges.AsNoTracking()
            .SingleAsync(r => r.Id == range.Id);

        Assert.Equal("provider verified worksheet", stored.Source);
        Assert.Equal(3m, stored.LowValue);
        Assert.Equal(7m, stored.HighValue);
        Assert.Equal(from, stored.EffectiveFromUtc);
        Assert.Equal(MasterDataStatus.Draft, stored.Status);
    }

    [Fact]
    public async Task LaboratoryData_IsInvisibleAcrossOrganizations()
    {
        using DbTestContextFactory factory = new(PersistenceDbFixture.ConnectionString);
        using CoreLabDbContext db = factory.Create();
        SeededTenant first = SeedSamples.Seed(db, $"lab-iso-a-{Guid.NewGuid():N}");
        SeededTenant second = SeedSamples.Seed(db, $"lab-iso-b-{Guid.NewGuid():N}");
        SeedSamples.SeedLaboratory(db, first);
        SeedSamples.SeedLaboratory(db, second);

        List<ConfigurationItem> firstItems = await db.ConfigurationItems.AsNoTracking()
            .Where(c => c.OrganizationId == first.OrganizationId)
            .ToListAsync();

        Assert.All(firstItems, item => Assert.Equal(first.OrganizationId, item.OrganizationId));
        Assert.DoesNotContain(firstItems, item => item.OrganizationId == second.OrganizationId);
    }
}
