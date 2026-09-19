using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Swasthya.CoreLabs.Domain.Orders;
using Swasthya.CoreLabs.Infrastructure.Persistence;
using Swasthya.CoreLabs.TestSupport;

namespace Swasthya.CoreLabs.Persistence.Tests;

[Trait("Category", TestCategory.Db)]
public sealed class OrderPersistenceTests : IClassFixture<PersistenceDbFixture>
{
    [Fact]
    public async Task Order_AndItems_RoundTripWithVersionPins()
    {
        using DbTestContextFactory factory = new(PersistenceDbFixture.ConnectionString);
        using CoreLabDbContext db = factory.Create();
        SeededTenant tenant = SeedSamples.Seed(db, $"ord-a-{Guid.NewGuid():N}");
        SeededLaboratory lab = SeedSamples.SeedLaboratory(db, tenant);
        SeededPanel panel = SeedSamples.SeedActivePanel(db, tenant, lab);
        SeededOrder seeded = SeedSamples.SeedOrder(db, tenant, lab, panel);

        Order order = await db.Orders.AsNoTracking()
            .SingleAsync(o => o.Id == seeded.OrderId);
        List<OrderItem> items = await db.OrderItems.AsNoTracking()
            .Where(i => i.OrderId == seeded.OrderId)
            .OrderBy(i => i.SequenceNumber)
            .ToListAsync();

        Assert.Equal(OrderStatus.Requested, order.Status);
        Assert.Equal(OrderPriority.Routine, order.Priority);
        Assert.Equal(tenant.FacilityAId, order.FacilityId);
        Assert.Equal("emr.test", order.PatientExternalSystem);
        Assert.Equal("subject-42", order.PatientExternalIdentifier);

        Assert.Equal(2, items.Count);
        Assert.NotNull(items[0].TestId);
        Assert.NotNull(items[0].TestVersionId);
        Assert.Null(items[0].PanelId);
        Assert.Null(items[0].PanelVersionId);
        Assert.NotNull(items[1].PanelId);
        Assert.NotNull(items[1].PanelVersionId);
        Assert.Null(items[1].TestId);
        Assert.Null(items[1].TestVersionId);
    }

    [Fact]
    public async Task Order_DuplicateOrderNumberWithinOrganization_Throws()
    {
        using DbTestContextFactory factory = new(PersistenceDbFixture.ConnectionString);
        using CoreLabDbContext db = factory.Create();
        SeededTenant tenant = SeedSamples.Seed(db, $"ord-dupno-{Guid.NewGuid():N}");
        SeededLaboratory lab = SeedSamples.SeedLaboratory(db, tenant);
        SeededPanel panel = SeedSamples.SeedActivePanel(db, tenant, lab);
        SeededOrder seeded = SeedSamples.SeedOrder(db, tenant, lab, panel);

        db.Orders.Add(new Order(
            tenant.OrganizationId,
            tenant.FacilityAId,
            seeded.OrderNumber,
            null,
            "emr.test",
            "subject-7"));

        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task Order_DuplicateOrderNumberInOtherOrganization_IsAllowed()
    {
        using DbTestContextFactory factory = new(PersistenceDbFixture.ConnectionString);
        using CoreLabDbContext db = factory.Create();
        SeededTenant first = SeedSamples.Seed(db, $"ord-xo-{Guid.NewGuid():N}");
        SeededTenant second = SeedSamples.Seed(db, $"ord-xo2-{Guid.NewGuid():N}");
        SeededLaboratory lab = SeedSamples.SeedLaboratory(db, first);
        SeededPanel panel = SeedSamples.SeedActivePanel(db, first, lab);
        SeededOrder seeded = SeedSamples.SeedOrder(db, first, lab, panel);

        db.Orders.Add(new Order(
            second.OrganizationId,
            second.FacilityAId,
            seeded.OrderNumber,
            null,
            "emr.test",
            "subject-7"));

        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task Order_DuplicateExternalOrderIdWithinOrganization_Throws()
    {
        using DbTestContextFactory factory = new(PersistenceDbFixture.ConnectionString);
        using CoreLabDbContext db = factory.Create();
        SeededTenant tenant = SeedSamples.Seed(db, $"ord-dupext-{Guid.NewGuid():N}");
        SeededLaboratory lab = SeedSamples.SeedLaboratory(db, tenant);
        SeededPanel panel = SeedSamples.SeedActivePanel(db, tenant, lab);
        SeedSamples.SeedOrder(db, tenant, lab, panel);
        string externalId = $"ext-{Guid.NewGuid():N}";

        var first = new Order(
            tenant.OrganizationId,
            tenant.FacilityAId,
            $"ord_{Guid.NewGuid():N}",
            externalId,
            "emr.test",
            "subject-1");
        var second = new Order(
            tenant.OrganizationId,
            tenant.FacilityAId,
            $"ord_{Guid.NewGuid():N}",
            externalId,
            "emr.test",
            "subject-2");
        db.Orders.AddRange(first, second);

        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public void OrderItem_SingleTargetCheckConstraint_IsEnforcedByDatabase()
    {
        using DbTestContextFactory factory = new(PersistenceDbFixture.ConnectionString);
        using CoreLabDbContext db = factory.Create();
        SeedSamples.Seed(db, $"ord-check-{Guid.NewGuid():N}");

        var designTimeModel = db.GetService<IDesignTimeModel>();
        var entity = designTimeModel.Model.FindEntityType(typeof(OrderItem))!;
        var check = entity.GetCheckConstraints();

        Assert.Contains(
            check,
            c => c.Name == "ck_order_items_single_target" && c.Sql.Contains("test_id"));
    }

    [Fact]
    public async Task OrderItem_UnknownTestVersionTarget_IsRejected()
    {
        using DbTestContextFactory factory = new(PersistenceDbFixture.ConnectionString);
        using CoreLabDbContext db = factory.Create();
        SeededTenant tenant = SeedSamples.Seed(db, $"ord-fk-{Guid.NewGuid():N}");

        db.OrderItems.Add(OrderItem.ForTest(
            Guid.NewGuid(), 0, Guid.NewGuid(), Guid.NewGuid(), "cbc", "CBC"));

        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task ConcurrentOrderCancel_IsRejected()
    {
        using DbTestContextFactory factory = new(PersistenceDbFixture.ConnectionString);
        SeededTenant tenant;
        SeededPanel panel;
        Guid orderId;
        using (CoreLabDbContext seed = factory.Create())
        {
            tenant = SeedSamples.Seed(seed, $"ord-conc-{Guid.NewGuid():N}");
            SeededLaboratory lab = SeedSamples.SeedLaboratory(seed, tenant);
            panel = SeedSamples.SeedActivePanel(seed, tenant, lab);
            SeededOrder seeded = SeedSamples.SeedOrder(seed, tenant, lab, panel);
            orderId = seeded.OrderId;
        }

        using CoreLabDbContext first = factory.Create();
        using CoreLabDbContext second = factory.Create();
        Order firstOrder = await first.Orders.SingleAsync(o => o.Id == orderId);
        Order secondOrder = await second.Orders.SingleAsync(o => o.Id == orderId);

        firstOrder.Cancel();
        await first.SaveChangesAsync();

        secondOrder.Cancel();

        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => second.SaveChangesAsync());
    }

    [Fact]
    public async Task Order_EnumsAndTimestampsPersistAsScalars()
    {
        using DbTestContextFactory factory = new(PersistenceDbFixture.ConnectionString);
        using CoreLabDbContext db = factory.Create();
        SeededTenant tenant = SeedSamples.Seed(db, $"ord-enum-{Guid.NewGuid():N}");
        SeededLaboratory lab = SeedSamples.SeedLaboratory(db, tenant);
        SeededPanel panel = SeedSamples.SeedActivePanel(db, tenant, lab);

        var order = new Order(
            tenant.OrganizationId,
            tenant.FacilityAId,
            $"ord_{Guid.NewGuid():N}",
            null,
            "emr.test",
            "subject-42",
            priority: OrderPriority.Stat);
        db.Orders.Add(order);
        await db.SaveChangesAsync();

        Order reloaded = await db.Orders.AsNoTracking()
            .SingleAsync(o => o.Id == order.Id);

        Assert.Equal(OrderPriority.Stat, reloaded.Priority);
        Assert.Equal(OrderStatus.Requested, reloaded.Status);
        Assert.True(reloaded.RequestedAtUtc <= DateTimeOffset.UtcNow);
        Assert.True(reloaded.Version > 0);
    }

    [Fact]
    public async Task Order_Update_RoundTripsRequestDetails()
    {
        using DbTestContextFactory factory = new(PersistenceDbFixture.ConnectionString);
        using CoreLabDbContext db = factory.Create();
        SeededTenant tenant = SeedSamples.Seed(db, $"ord-upd-{Guid.NewGuid():N}");
        SeededLaboratory lab = SeedSamples.SeedLaboratory(db, tenant);
        SeededPanel panel = SeedSamples.SeedActivePanel(db, tenant, lab);
        SeededOrder seeded = SeedSamples.SeedOrder(db, tenant, lab, panel);

        Order order = await db.Orders.SingleAsync(o => o.Id == seeded.OrderId);
        order.Update(
            OrderPriority.Urgent,
            "ehr.next",
            "subject-99",
            "emr.test",
            "encounter-7");
        await db.SaveChangesAsync();

        Order reloaded = await db.Orders.AsNoTracking()
            .SingleAsync(o => o.Id == seeded.OrderId);

        Assert.Equal(OrderPriority.Urgent, reloaded.Priority);
        Assert.Equal("ehr.next", reloaded.PatientExternalSystem);
        Assert.Equal("subject-99", reloaded.PatientExternalIdentifier);
        Assert.Equal("emr.test", reloaded.EncounterExternalSystem);
        Assert.Equal("encounter-7", reloaded.EncounterExternalIdentifier);
    }

    [Fact]
    public async Task Order_ListIsOrganizationScoped()
    {
        using DbTestContextFactory factory = new(PersistenceDbFixture.ConnectionString);
        using CoreLabDbContext db = factory.Create();
        SeededTenant first = SeedSamples.Seed(db, $"ord-scope-{Guid.NewGuid():N}");
        SeededTenant second = SeedSamples.Seed(db, $"ord-scope2-{Guid.NewGuid():N}");
        SeededLaboratory lab = SeedSamples.SeedLaboratory(db, first);
        SeededPanel panel = SeedSamples.SeedActivePanel(db, first, lab);
        SeedSamples.SeedOrder(db, first, lab, panel);

        List<Order> firstOrders = await db.Orders.AsNoTracking()
            .Where(o => o.OrganizationId == first.OrganizationId)
            .ToListAsync();
        List<Order> secondOrders = await db.Orders.AsNoTracking()
            .Where(o => o.OrganizationId == second.OrganizationId)
            .ToListAsync();

        Assert.Single(firstOrders);
        Assert.Empty(secondOrders);
    }
}
