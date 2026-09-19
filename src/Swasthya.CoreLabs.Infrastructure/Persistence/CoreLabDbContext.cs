using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using Swasthya.CoreLabs.Domain.Audit;
using Swasthya.CoreLabs.Domain.IdentityTenancy;
using Swasthya.CoreLabs.Domain.Laboratory;
using Swasthya.CoreLabs.Domain.Orders;
using Swasthya.CoreLabs.Domain.Results;
using Swasthya.CoreLabs.Domain.Specimen;

namespace Swasthya.CoreLabs.Infrastructure.Persistence;

public sealed class CoreLabDbContext(DbContextOptions<CoreLabDbContext> options) : DbContext(options)
{
    public DbSet<Organization> Organizations => Set<Organization>();

    public DbSet<Facility> Facilities => Set<Facility>();

    public DbSet<Principal> Principals => Set<Principal>();

    public DbSet<Permission> Permissions => Set<Permission>();

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    public DbSet<PrincipalRoleAssignment> PrincipalRoleAssignments => Set<PrincipalRoleAssignment>();

    public DbSet<AuditRecord> AuditRecords => Set<AuditRecord>();

    public DbSet<Discipline> Disciplines => Set<Discipline>();

    public DbSet<LaboratorySection> LaboratorySections => Set<LaboratorySection>();

    public DbSet<ResultDataType> ResultDataTypes => Set<ResultDataType>();

    public DbSet<Unit> Units => Set<Unit>();

    public DbSet<CodeSystem> CodeSystems => Set<CodeSystem>();

    public DbSet<Specimen> Specimens => Set<Specimen>();

    public DbSet<Accession> Accessions => Set<Accession>();

    public DbSet<SpecimenRequirement> SpecimenRequirements => Set<SpecimenRequirement>();

    public DbSet<Container> Containers => Set<Container>();

    public DbSet<SpecimenSource> SpecimenSources => Set<SpecimenSource>();

    public DbSet<Swasthya.CoreLabs.Domain.Laboratory.SpecimenType> SpecimenTypes => Set<Swasthya.CoreLabs.Domain.Laboratory.SpecimenType>();

    public DbSet<Swasthya.CoreLabs.Domain.Laboratory.ContainerType> ContainerTypes => Set<Swasthya.CoreLabs.Domain.Laboratory.ContainerType>();

    public DbSet<Result> Results => Set<Result>();

    public DbSet<ResultStatus> ResultStatuses => Set<ResultStatus>();

    public DbSet<Analyzer> Analyzers => Set<Analyzer>();

    public DbSet<ConfigurationItem> ConfigurationItems => Set<ConfigurationItem>();

    public DbSet<Test> Tests => Set<Test>();

    public DbSet<TestVersion> TestVersions => Set<TestVersion>();

    public DbSet<TestSpecimenRequirement> TestSpecimenRequirements => Set<TestSpecimenRequirement>();

    public DbSet<TestTerminologyMapping> TestTerminologyMappings => Set<TestTerminologyMapping>();

    public DbSet<TestFacility> TestFacilities => Set<TestFacility>();

    public DbSet<Panel> Panels => Set<Panel>();

    public DbSet<PanelVersion> PanelVersions => Set<PanelVersion>();

    public DbSet<PanelTestMembership> PanelTestMemberships => Set<PanelTestMembership>();

    public DbSet<ReferenceRange> ReferenceRanges => Set<ReferenceRange>();

    public DbSet<Order> Orders => Set<Order>();

    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CoreLabDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
