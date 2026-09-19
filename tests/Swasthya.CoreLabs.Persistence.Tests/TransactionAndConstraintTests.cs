using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;
using Swasthya.CoreLabs.Domain.Common;
using Swasthya.CoreLabs.Domain.IdentityTenancy;
using Swasthya.CoreLabs.Infrastructure.Persistence;
using Swasthya.CoreLabs.TestSupport;

namespace Swasthya.CoreLabs.Persistence.Tests;

[Trait("Category", TestCategory.Db)]
public sealed class TransactionAndConstraintTests : IClassFixture<PersistenceDbFixture>
{
    [Fact]
    public async Task Transaction_Commit_PersistsAcrossContext()
    {
        using DbTestContextFactory factory = new(PersistenceDbFixture.ConnectionString);
        Guid id;
        string name = $"tx-commit-{Guid.NewGuid():N}";
        using (CoreLabDbContext db = factory.Create())
        {
            await using IDbContextTransaction transaction =
                await db.Database.BeginTransactionAsync();
            var org = new Organization(name);
            db.Organizations.Add(org);
            await db.SaveChangesAsync();
            await transaction.CommitAsync();
            id = org.Id;
        }

        using CoreLabDbContext verify = factory.Create();
        Organization? reloaded = await verify.Organizations.SingleOrDefaultAsync(o => o.Id == id);
        Assert.NotNull(reloaded);
        Assert.Equal(name, reloaded.Name);
    }

    [Fact]
    public async Task Transaction_Rollback_DiscardsAllChanges()
    {
        using DbTestContextFactory factory = new(PersistenceDbFixture.ConnectionString);
        Guid id;
        string name = $"tx-rollback-{Guid.NewGuid():N}";
        using (CoreLabDbContext db = factory.Create())
        {
            await using IDbContextTransaction transaction =
                await db.Database.BeginTransactionAsync();
            var org = new Organization(name);
            db.Organizations.Add(org);
            await db.SaveChangesAsync();
            id = org.Id;

            db.Organizations.Add(new Organization(name));
            await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());

            await transaction.RollbackAsync();
        }

        using CoreLabDbContext verify = factory.Create();
        Assert.Null(await verify.Organizations.SingleOrDefaultAsync(o => o.Id == id));
        Assert.Empty(await verify.Organizations.Where(o => o.Name == name).ToListAsync());
    }

    [Fact]
    public async Task SaveChanges_WithFailingInsert_IsAtomic()
    {
        using DbTestContextFactory factory = new(PersistenceDbFixture.ConnectionString);
        using CoreLabDbContext db = factory.Create();
        string name = $"atomic-{Guid.NewGuid():N}";
        db.Organizations.Add(new Organization(name));
        db.Organizations.Add(new Organization(name));

        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());

        Assert.Empty(await db.Organizations.Where(o => o.Name == name).ToListAsync());
    }

    [Fact]
    public async Task ForeignKey_InvalidRoleReference_IsRejected()
    {
        using DbTestContextFactory factory = new(PersistenceDbFixture.ConnectionString);
        using CoreLabDbContext db = factory.Create();
        var seeded = SeedSamples.Seed(db, $"fk-{Guid.NewGuid():N}");
        var principal = new Principal(
            PrincipalType.Human, TestJwtDefaults.Issuer, $"fk-p-{Guid.NewGuid():N}", "FK");
        db.Principals.Add(principal);
        await db.SaveChangesAsync();

        db.PrincipalRoleAssignments.Add(new PrincipalRoleAssignment(
            principal.Id, Guid.NewGuid(), seeded.OrganizationId));

        DbUpdateException exception = await Assert.ThrowsAsync<DbUpdateException>(
            () => db.SaveChangesAsync());

        PostgresException? inner = exception.InnerException as PostgresException;
        Assert.NotNull(inner);
        Assert.Equal("23503", inner!.SqlState);
    }
}
