using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Swasthya.CoreLabs.TestSupport;

namespace Swasthya.CoreLabs.Api.Tests;

public class SwasthyaApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("Authentication:Issuer", TestJwtDefaults.Issuer);
        builder.UseSetting("Authentication:Audience", TestJwtDefaults.Audience);
        builder.UseSetting("Authentication:SigningKeyBase64", TestJwtDefaults.SigningKeyBase64);
        builder.UseSetting("Authentication:PrincipalTypeClaim", "principal_type");
        builder.UseSetting("Authentication:ServicePrincipalTypeValue", "service");
    }
}

public sealed class SwasthyaDbApiFactory : SwasthyaApiFactory
{
    private static readonly object MigrateLock = new();

    private static bool _migrated;

    public SwasthyaDbApiFactory()
    {
        using (var factory = new DbTestContextFactory(ConnectionString))
        {
            lock (MigrateLock)
            {
                if (!_migrated)
                {
                    factory.EnsureDatabase();
                    _migrated = true;
                }
            }
        }
    }

    public string ConnectionString { get; } = TestJwtDefaults.GetConnectionString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.UseSetting("ConnectionStrings:CoreLabs", ConnectionString);
    }
}
