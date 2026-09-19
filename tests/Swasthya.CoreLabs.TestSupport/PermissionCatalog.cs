using Swasthya.CoreLabs.Domain.IdentityTenancy;
using Swasthya.CoreLabs.Infrastructure.Persistence;

namespace Swasthya.CoreLabs.TestSupport;

public static class PermissionCatalog
{
    private static readonly object Sync = new();

    private static bool _ready;

    private static readonly string[] Codes =
    [
        "organization.list",
        "facility.list",
        "audit.read",
        "identity.principal.read",
        "laboratory.catalog.read",
        "laboratory.catalog.write",
        "laboratory.analyzer.write",
        "laboratory.config.write",
        "laboratory.order.read",
        "laboratory.order.write",
        "laborarium.specimen.read",
        "laborarium.specimen.write",
        "laborarium.accession.read",
        "laborarium.accession.write",
    ];

    public static IReadOnlyDictionary<string, Guid> Ensure(CoreLabDbContext db)
    {
        lock (Sync)
        {
            if (!_ready)
            {
                foreach (string code in Codes)
                {
                    if (!db.Permissions.Any(p => p.Code == code))
                    {
                        db.Permissions.Add(new Permission(code, $"Permission '{code}'"));
                    }
                }

                db.SaveChanges();
                _ready = true;
            }
        }

        return db.Permissions
            .Where(p => Codes.Contains(p.Code))
            .ToDictionary(p => p.Code, p => p.Id);
    }
}
