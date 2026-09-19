using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Swasthya.CoreLabs.Infrastructure.Persistence;

public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<CoreLabDbContext>
{
    public CoreLabDbContext CreateDbContext(string[] args)
    {
        string? connectionString = Environment.GetEnvironmentVariable("SCL_DB_CONNECTION");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Set the SCL_DB_CONNECTION environment variable to run EF Core design-time commands.");
        }

        DbContextOptions<CoreLabDbContext> options = new DbContextOptionsBuilder<CoreLabDbContext>()
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention()
            .Options;

        return new CoreLabDbContext(options);
    }
}
