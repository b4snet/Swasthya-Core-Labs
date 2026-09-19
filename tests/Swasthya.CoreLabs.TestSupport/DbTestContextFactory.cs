using Microsoft.EntityFrameworkCore;
using Swasthya.CoreLabs.Infrastructure.Persistence;

namespace Swasthya.CoreLabs.TestSupport;

public sealed class DbTestContextFactory(string connectionString) : IDisposable
{
    public CoreLabDbContext Create()
    {
        DbContextOptions<CoreLabDbContext> options = new DbContextOptionsBuilder<CoreLabDbContext>()
            .UseNpgsql(connectionString, npgsql => npgsql.MigrationsAssembly(typeof(CoreLabDbContext).Assembly.FullName))
            .UseSnakeCaseNamingConvention()
            .Options;

        return new CoreLabDbContext(options);
    }

    public void EnsureDatabase()
    {
        using CoreLabDbContext context = Create();
        context.Database.Migrate();
    }

    public void Dispose()
    {
    }
}
