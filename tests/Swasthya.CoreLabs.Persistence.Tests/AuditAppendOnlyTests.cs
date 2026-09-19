using Microsoft.EntityFrameworkCore;
using Npgsql;
using Swasthya.CoreLabs.Application.Abstractions;
using Swasthya.CoreLabs.Domain.Audit;
using Swasthya.CoreLabs.Domain.Common;
using Swasthya.CoreLabs.Infrastructure.Persistence;
using Swasthya.CoreLabs.TestSupport;

namespace Swasthya.CoreLabs.Persistence.Tests;

[Trait("Category", TestCategory.Db)]
public sealed class AuditAppendOnlyTests : IClassFixture<PersistenceDbFixture>
{
    [Fact]
    public async Task AuditRecord_Insert_IsAllowed()
    {
        using DbTestContextFactory factory = new(PersistenceDbFixture.ConnectionString);
        using CoreLabDbContext db = factory.Create();
        var seeded = SeedSamples.Seed(db, $"aud-ins-{Guid.NewGuid():N}");
        var record = new AuditRecord(
            seeded.OrganizationId,
            "seed.record.insert",
            AuditResourceTypes.AuditRecord,
            null,
            AuditOutcome.Success,
            $"corr-{Guid.NewGuid():N}",
            seeded.AdminPrincipalId,
            PrincipalType.Human);

        db.AuditRecords.Add(record);
        await db.SaveChangesAsync();

        Assert.True(record.Id != Guid.Empty);
        Assert.Single(db.AuditRecords.Where(a => a.Id == record.Id));
    }

    [Fact]
    public async Task AuditRecord_Update_IsBlocked()
    {
        Guid recordId = await InsertAuditRecordAsync();

        PostgresException exception = await Assert.ThrowsAsync<PostgresException>(async () =>
        {
            using var connection = new NpgsqlConnection(PersistenceDbFixture.ConnectionString);
            await connection.OpenAsync();
            using var command = new NpgsqlCommand(
                "update audit_records set outcome = @outcome where id = @id", connection);
            command.Parameters.AddWithValue("outcome", 1);
            command.Parameters.AddWithValue("id", recordId);
            await command.ExecuteNonQueryAsync();
        });

        Assert.Equal("55000", exception.SqlState);
    }

    [Fact]
    public async Task AuditRecord_Delete_IsBlocked()
    {
        Guid recordId = await InsertAuditRecordAsync();

        PostgresException exception = await Assert.ThrowsAsync<PostgresException>(async () =>
        {
            using var connection = new NpgsqlConnection(PersistenceDbFixture.ConnectionString);
            await connection.OpenAsync();
            using var command = new NpgsqlCommand(
                "delete from audit_records where id = @id", connection);
            command.Parameters.AddWithValue("id", recordId);
            await command.ExecuteNonQueryAsync();
        });

        Assert.Equal("55000", exception.SqlState);
    }

    [Fact]
    public async Task AuditRecord_SurvivesBlockedMutation()
    {
        Guid recordId = await InsertAuditRecordAsync();
        using var connection = new NpgsqlConnection(PersistenceDbFixture.ConnectionString);
        await connection.OpenAsync();
        using var command = new NpgsqlCommand(
            "delete from audit_records where id = @id", connection);
        command.Parameters.AddWithValue("id", recordId);
        await Assert.ThrowsAsync<PostgresException>(
            async () => await command.ExecuteNonQueryAsync());

        using DbTestContextFactory factory = new(PersistenceDbFixture.ConnectionString);
        using CoreLabDbContext db = factory.Create();
        Assert.Equal(1, await db.AuditRecords.CountAsync(a => a.Id == recordId));
    }

    private static async Task<Guid> InsertAuditRecordAsync()
    {
        using DbTestContextFactory factory = new(PersistenceDbFixture.ConnectionString);
        using CoreLabDbContext db = factory.Create();
        var seeded = SeedSamples.Seed(db, $"aud-mut-{Guid.NewGuid():N}");
        var record = new AuditRecord(
            seeded.OrganizationId,
            "seed.record.mutation",
            AuditResourceTypes.AuditRecord,
            null,
            AuditOutcome.Success,
            $"corr-{Guid.NewGuid():N}",
            seeded.AdminPrincipalId,
            PrincipalType.Human);
        db.AuditRecords.Add(record);
        await db.SaveChangesAsync();
        return record.Id;
    }
}
