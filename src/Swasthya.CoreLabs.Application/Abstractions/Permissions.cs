namespace Swasthya.CoreLabs.Application.Abstractions;

public static class Permissions
{
    public const string OrganizationList = "organization.list";
    public const string FacilityList = "facility.list";
    public const string PrincipalRead = "identity.principal.read";
    public const string AuditRead = "audit.read";

    /// <summary>Read access to laboratory configuration and master data.</summary>
    public const string LaboratoryCatalogRead = "laboratory.catalog.read";

    /// <summary>Write access to laboratory configuration and master data (tests, panels, reference ranges).</summary>
    public const string LaboratoryCatalogWrite = "laboratory.catalog.write";

    /// <summary>Write access to specimen and accession lifecycle.</summary>
    public const string SpecimenWrite = "laborarium.specimen.write";

    /// <summary>Write access to analyzer/device registry configuration.</summary>
    public const string LaboratoryAnalyzerWrite = "laboratory.analyzer.write";

    /// <summary>Write access to laboratory operational configuration items.</summary>
    public const string LaboratoryConfigWrite = "laboratory.config.write";

    /// <summary>Read access to diagnostic orders and order items.</summary>
    public const string OrderRead = "laboratory.order.read";

    /// <summary>Write access to diagnostic orders (create, update, cancel, add items).</summary>
    public const string OrderWrite = "laboratory.order.write";

    /// <summary>Verifies a result (explicit persisted transition requiring applicable validation).</summary>
    public const string LaboratoryResultVerify = "laboratory.result.verify";

    /// <finalizes a result (explicit transition requiring verification).</summary>
    public const string LaboratoryResultFinalize = "laboratory.result.finalize";
}
