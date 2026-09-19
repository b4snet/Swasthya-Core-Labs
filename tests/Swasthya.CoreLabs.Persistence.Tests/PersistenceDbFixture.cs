using Swasthya.CoreLabs.TestSupport;

namespace Swasthya.CoreLabs.Persistence.Tests;

[Trait("Category", TestCategory.Db)]
public sealed class PersistenceDbFixture
{
    public PersistenceDbFixture()
    {
        using (var factory = new DbTestContextFactory(ConnectionString))
        {
            factory.EnsureDatabase();
        }
    }

    public static string ConnectionString { get; } = TestJwtDefaults.GetConnectionString();
}
