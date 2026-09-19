using Microsoft.EntityFrameworkCore;
using Swasthya.CoreLabs.Infrastructure.Persistence;
using Swasthya.CoreLabs.TestSupport;

namespace Swasthya.CoreLabs.Persistence.Tests;

[Trait("Category", TestCategory.Db)]
public sealed class ConcurrencyTests : IClassFixture<PersistenceDbFixture>
{
    [Fact]
    public async Task OptimisticConcurrency_SecondWriter_Throws()
    {
        using DbTestContextFactory factory = new(PersistenceDbFixture.ConnectionString);
        Guid orgId;
        using (CoreLabDbContext db = factory.Create())
        {
            var seeded = SeedSamples.Seed(db, $"cc-{Guid.NewGuid():N}");
            orgId = seeded.OrganizationId;
        }

        using CoreLabDbContext firstContext = factory.Create();
        using CoreLabDbContext secondContext = factory.Create();
        var first = await firstContext.Organizations.SingleAsync(o => o.Id == orgId);
        var second = await secondContext.Organizations.SingleAsync(o => o.Id == orgId);

        first.Rename($"First writer {Guid.NewGuid():N}");
        await firstContext.SaveChangesAsync();

        second.Rename($"Second writer {Guid.NewGuid():N}");
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(
            () => secondContext.SaveChangesAsync());
    }

    [Fact]
    public async Task SingleWriter_UpdatesSuccessfully()
    {
        using DbTestContextFactory factory = new(PersistenceDbFixture.ConnectionString);
        using CoreLabDbContext db = factory.Create();
        var seeded = SeedSamples.Seed(db, $"cc-ok-{Guid.NewGuid():N}");
        var org = await db.Organizations.SingleAsync(o => o.Id == seeded.OrganizationId);

        org.Rename($"Renamed {Guid.NewGuid():N}");
        await db.SaveChangesAsync();

        Assert.Equal(EntityState.Unchanged, db.Entry(org).State);
    }
}
