using Swasthya.CoreLabs.Application.Abstractions;
using Swasthya.CoreLabs.Domain.Audit;
using Swasthya.CoreLabs.Domain.Common;
using Swasthya.CoreLabs.Domain.IdentityTenancy;
using Swasthya.CoreLabs.Domain.Laboratory;
using Swasthya.CoreLabs.Domain.Orders;
using Swasthya.CoreLabs.Infrastructure.Persistence;

namespace Swasthya.CoreLabs.TestSupport;

public sealed record SeededTenant(
    Guid OrganizationId,
    Guid FacilityAId,
    Guid FacilityBId,
    Guid AdminPrincipalId,
    string AdminSubject,
    Guid StaffPrincipalId,
    string StaffSubject,
    Guid ServiceAuditorPrincipalId,
    string ServiceAuditorSubject,
    Guid AdminRoleId,
    Guid StaffRoleId);

public sealed record SeededLaboratory(
    Guid DisciplineId,
    Guid ResultDataTypeId,
    Guid UnitId,
    Guid CodeSystemId,
    Guid SpecimenTypeId,
    Guid SpecimenSourceId,
    Guid ContainerTypeId,
    Guid ResultStatusId,
    Guid SectionId,
    Guid AnalyzerId,
    Guid ConfigurationItemId,
    Guid TestId,
    string TestCode);

public sealed record SeededPanel(
    Guid PanelId,
    string PanelCode);

public sealed record SeededOrder(
    Guid OrderId,
    string OrderNumber,
    Guid TestItemId,
    Guid PanelItemId);

public static class SeedSamples
{
    public static SeededTenant Seed(CoreLabDbContext db, string label)
    {
        string token = Guid.NewGuid().ToString("N")[..8];
        IReadOnlyDictionary<string, Guid> permissions = PermissionCatalog.Ensure(db);

        var org = new Organization($"Org {label} {token}");
        var facilityA = new Facility(org.Id, $"FA{token}", $"Facility A {label} {token}");
        var facilityB = new Facility(org.Id, $"FB{token}", $"Facility B {label} {token}");

        var roleOrgAdmin = new Role(org.Id, $"org_admin_{token}", "Organization admin");
        var roleFacilityA = new Role(org.Id, $"facility_a_{token}", "Facility A staff");
        var roleAuditor = new Role(org.Id, $"auditor_{token}", "Auditor");

        string adminSubject = $"admin.{token}";
        string staffSubject = $"staff.{token}";
        string auditorSubject = $"svc.{token}";

        var admin = new Principal(
            PrincipalType.Human,
            TestJwtDefaults.Issuer,
            adminSubject,
            $"Admin {label}");
        var staff = new Principal(
            PrincipalType.Human,
            TestJwtDefaults.Issuer,
            staffSubject,
            $"Staff {label}");
        var auditor = new Principal(
            PrincipalType.Service,
            TestJwtDefaults.Issuer,
            auditorSubject,
            $"Auditor {label}");

        db.Organizations.Add(org);
        db.Facilities.AddRange(facilityA, facilityB);
        db.Roles.AddRange(roleOrgAdmin, roleFacilityA, roleAuditor);
        db.Principals.AddRange(admin, staff, auditor);

        db.RolePermissions.AddRange(
            new RolePermission(roleOrgAdmin.Id, permissions["organization.list"]),
            new RolePermission(roleOrgAdmin.Id, permissions["facility.list"]),
            new RolePermission(roleOrgAdmin.Id, permissions["audit.read"]),
            new RolePermission(roleOrgAdmin.Id, permissions["identity.principal.read"]),
            new RolePermission(roleFacilityA.Id, permissions["organization.list"]),
            new RolePermission(roleFacilityA.Id, permissions["facility.list"]),
            new RolePermission(roleFacilityA.Id, permissions["audit.read"]),
            new RolePermission(roleAuditor.Id, permissions["audit.read"]));

        db.PrincipalRoleAssignments.AddRange(
            new PrincipalRoleAssignment(admin.Id, roleOrgAdmin.Id, org.Id),
            new PrincipalRoleAssignment(staff.Id, roleFacilityA.Id, org.Id, facilityA.Id),
            new PrincipalRoleAssignment(auditor.Id, roleAuditor.Id, org.Id));

        db.SaveChanges();

        return new SeededTenant(
            org.Id,
            facilityA.Id,
            facilityB.Id,
            admin.Id,
            adminSubject,
            staff.Id,
            staffSubject,
            auditor.Id,
            auditorSubject,
            roleOrgAdmin.Id,
            roleFacilityA.Id);
    }

    public static void GrantLaboratoryPermissions(CoreLabDbContext db, SeededTenant tenant)
    {
        IReadOnlyDictionary<string, Guid> permissions = PermissionCatalog.Ensure(db);

        db.RolePermissions.AddRange(
            new RolePermission(tenant.AdminRoleId, permissions["laboratory.catalog.read"]),
            new RolePermission(tenant.AdminRoleId, permissions["laboratory.catalog.write"]),
            new RolePermission(tenant.AdminRoleId, permissions["laboratory.analyzer.write"]),
            new RolePermission(tenant.AdminRoleId, permissions["laboratory.config.write"]),
            new RolePermission(tenant.StaffRoleId, permissions["laboratory.catalog.read"]),
            new RolePermission(tenant.StaffRoleId, permissions["laboratory.config.write"]));

        db.SaveChanges();
    }

    public static void GrantOrderPermissions(CoreLabDbContext db, SeededTenant tenant)
    {
        IReadOnlyDictionary<string, Guid> permissions = PermissionCatalog.Ensure(db);

        db.RolePermissions.AddRange(
            new RolePermission(tenant.AdminRoleId, permissions["laboratory.order.read"]),
            new RolePermission(tenant.AdminRoleId, permissions["laboratory.order.write"]),
            new RolePermission(tenant.StaffRoleId, permissions["laboratory.order.read"]),
            new RolePermission(tenant.StaffRoleId, permissions["laboratory.order.write"]));

        db.SaveChanges();
    }

    public static void ActivateSeededTest(CoreLabDbContext db, SeededLaboratory lab)
    {
        Test test = db.Tests.Single(t => t.Id == lab.TestId);
        test.SetStatus(MasterDataStatus.Active);
        db.SaveChanges();
    }

    public static SeededPanel SeedActivePanel(
        CoreLabDbContext db,
        SeededTenant tenant,
        SeededLaboratory lab)
    {
        string token = Guid.NewGuid().ToString("N")[..8];

        var panel = new Panel(tenant.OrganizationId, $"panel_{token}");
        int versionNumber = panel.StartNextVersion();
        panel.SetStatus(MasterDataStatus.Active);
        db.Panels.Add(panel);
        db.PanelVersions.Add(new PanelVersion(panel.Id, versionNumber, $"Panel {token}"));
        db.PanelTestMemberships.Add(new PanelTestMembership(panel.Id, lab.TestId, 1));
        db.SaveChanges();

        return new SeededPanel(panel.Id, panel.Code);
    }

    public static SeededOrder SeedOrder(
        CoreLabDbContext db,
        SeededTenant tenant,
        SeededLaboratory lab,
        SeededPanel panel)
    {
        TestVersion testVersion = db.TestVersions.Single(v => v.TestId == lab.TestId);
        PanelVersion panelVersion = db.PanelVersions.Single(v => v.PanelId == panel.PanelId);

        var order = new Order(
            tenant.OrganizationId,
            tenant.FacilityAId,
            $"ord_{Guid.NewGuid():N}",
            null,
            "emr.test",
            "subject-42");
        db.Orders.Add(order);

        OrderItem testItem = OrderItem.ForTest(
            order.Id, 0, lab.TestId, testVersion.Id, lab.TestCode, $"Test {Guid.NewGuid():N}");
        OrderItem panelItem = OrderItem.ForPanel(
            order.Id, 1, panel.PanelId, panelVersion.Id, panel.PanelCode, "Panel");
        db.OrderItems.AddRange(testItem, panelItem);
        db.SaveChanges();

        return new SeededOrder(order.Id, order.OrderNumber, testItem.Id, panelItem.Id);
    }

    public static SeededLaboratory SeedLaboratory(
        CoreLabDbContext db,
        SeededTenant tenant)
    {
        string token = Guid.NewGuid().ToString("N")[..8];

        var discipline = new Discipline(
            tenant.OrganizationId, $"disc_{token}", $"Discipline {token}");
        var dataType = new ResultDataType(
            tenant.OrganizationId,
            $"quant_{token}",
            $"Quantitative {token}",
            ResultDataTypeKind.Quantitative);
        var unit = new Unit(
            tenant.OrganizationId, $"unit_{token}", $"Unit {token}", "mg/dL");
        var codeSystem = new CodeSystem(
            tenant.OrganizationId,
            $"cs_{token}",
            $"Code system {token}",
            "https://example.test/cs",
            true);
        var specimenType = new SpecimenType(
            tenant.OrganizationId, $"spec_{token}", $"Specimen {token}");
        var specimenSource = new SpecimenSource(
            tenant.OrganizationId, $"src_{token}", $"Source {token}");
        var containerType = new ContainerType(
            tenant.OrganizationId, $"cont_{token}", $"Container {token}");
        var resultStatus = new ResultStatus(
            tenant.OrganizationId, $"rs_{token}", $"Status {token}");
        var section = new LaboratorySection(
            tenant.OrganizationId, tenant.FacilityAId, $"sec_{token}", $"Section {token}");
        var analyzer = new Analyzer(
            tenant.OrganizationId,
            tenant.FacilityAId,
            $"an_{token}",
            $"Analyzer {token}");
        var configuration = new ConfigurationItem(
            tenant.OrganizationId,
            null,
            $"lab.{token}.mode",
            "standard",
            ConfigurationValueType.Text);

        db.Disciplines.Add(discipline);
        db.ResultDataTypes.Add(dataType);
        db.Units.Add(unit);
        db.CodeSystems.Add(codeSystem);
        db.SpecimenTypes.Add(specimenType);
        db.SpecimenSources.Add(specimenSource);
        db.ContainerTypes.Add(containerType);
        db.ResultStatuses.Add(resultStatus);
        db.LaboratorySections.Add(section);
        db.Analyzers.Add(analyzer);
        db.ConfigurationItems.Add(configuration);
        db.SaveChanges();

        var test = new Test(tenant.OrganizationId, $"test_{token}");
        db.Tests.Add(test);
        int versionNumber = test.StartNextVersion();
        var version = new TestVersion(
            test.Id, versionNumber, $"Test {token}", dataType.Id, discipline.Id, unit.Id);

        db.TestVersions.Add(version);
        db.SaveChanges();

        return new SeededLaboratory(
            discipline.Id,
            dataType.Id,
            unit.Id,
            codeSystem.Id,
            specimenType.Id,
            specimenSource.Id,
            containerType.Id,
            resultStatus.Id,
            section.Id,
            analyzer.Id,
            configuration.Id,
            test.Id,
            test.Code);
    }

    public static Guid CreateUnassignedPrincipal(CoreLabDbContext db, string suffix)
    {
        var principal = new Principal(
            PrincipalType.Human,
            TestJwtDefaults.Issuer,
            $"unassigned.{suffix}",
            "Unassigned principal");
        db.Principals.Add(principal);
        db.SaveChanges();
        return principal.Id;
    }

    public static void AddAuditRecord(
        CoreLabDbContext db,
        Guid organizationId,
        Guid? facilityId,
        Guid? actorId,
        PrincipalType? actorType,
        string action,
        string correlationId)
    {
        db.AuditRecords.Add(new AuditRecord(
            organizationId,
            action,
            AuditResourceTypes.AuditRecord,
            null,
            AuditOutcome.Success,
            correlationId,
            actorId,
            actorType,
            facilityId,
            null));
        db.SaveChanges();
    }
}
