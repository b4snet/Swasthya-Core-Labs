using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Swasthya.CoreLabs.Domain.IdentityTenancy;
using Swasthya.CoreLabs.Domain.Laboratory;

namespace Swasthya.CoreLabs.Infrastructure.Persistence;

internal sealed class DisciplineConfiguration : IEntityTypeConfiguration<Discipline>
{
    public void Configure(EntityTypeBuilder<Discipline> builder)
    {
        builder.ToTable("disciplines");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(d => d.Code).IsRequired().HasMaxLength(100);
        builder.Property(d => d.Name).IsRequired().HasMaxLength(150);
        builder.Property(d => d.Description).IsRequired().HasMaxLength(500);
        builder.HasIndex(d => new { d.OrganizationId, d.Code }).IsUnique();
        builder.HasOne<Organization>()
            .WithMany()
            .HasForeignKey(d => d.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Property(d => d.Version).IsRowVersion();
    }
}

internal sealed class LaboratorySectionConfiguration : IEntityTypeConfiguration<LaboratorySection>
{
    public void Configure(EntityTypeBuilder<LaboratorySection> builder)
    {
        builder.ToTable("laboratory_sections");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(s => s.Code).IsRequired().HasMaxLength(100);
        builder.Property(s => s.Name).IsRequired().HasMaxLength(150);
        builder.Property(s => s.Description).IsRequired().HasMaxLength(500);
        builder.HasIndex(s => new { s.OrganizationId, s.FacilityId, s.Code }).IsUnique();
        builder.HasOne<Organization>()
            .WithMany()
            .HasForeignKey(s => s.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Facility>()
            .WithMany()
            .HasForeignKey(s => s.FacilityId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Property(s => s.Version).IsRowVersion();
    }
}

internal sealed class ResultDataTypeConfiguration : IEntityTypeConfiguration<ResultDataType>
{
    public void Configure(EntityTypeBuilder<ResultDataType> builder)
    {
        builder.ToTable("result_data_types");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(r => r.Code).IsRequired().HasMaxLength(100);
        builder.Property(r => r.Name).IsRequired().HasMaxLength(150);
        builder.Property(r => r.Description).IsRequired().HasMaxLength(500);
        builder.HasIndex(r => new { r.OrganizationId, r.Code }).IsUnique();
        builder.HasOne<Organization>()
            .WithMany()
            .HasForeignKey(r => r.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Property(r => r.Version).IsRowVersion();
    }
}

internal sealed class UnitConfiguration : IEntityTypeConfiguration<Unit>
{
    public void Configure(EntityTypeBuilder<Unit> builder)
    {
        builder.ToTable("units");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(u => u.Code).IsRequired().HasMaxLength(100);
        builder.Property(u => u.Name).IsRequired().HasMaxLength(150);
        builder.Property(u => u.UcumCode).IsRequired().HasMaxLength(80);
        builder.Property(u => u.Description).IsRequired().HasMaxLength(500);
        builder.HasIndex(u => new { u.OrganizationId, u.Code }).IsUnique();
        builder.HasOne<Organization>()
            .WithMany()
            .HasForeignKey(u => u.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Property(u => u.Version).IsRowVersion();
    }
}

internal sealed class CodeSystemConfiguration : IEntityTypeConfiguration<CodeSystem>
{
    public void Configure(EntityTypeBuilder<CodeSystem> builder)
    {
        builder.ToTable("code_systems");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(c => c.Code).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Name).IsRequired().HasMaxLength(150);
        builder.Property(c => c.CanonicalUrl).IsRequired().HasMaxLength(300);
        builder.Property(c => c.Description).IsRequired().HasMaxLength(500);
        builder.HasIndex(c => new { c.OrganizationId, c.Code }).IsUnique();
        builder.HasOne<Organization>()
            .WithMany()
            .HasForeignKey(c => c.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Property(c => c.Version).IsRowVersion();
    }
}

internal sealed class SpecimenTypeConfiguration : IEntityTypeConfiguration<SpecimenType>
{
    public void Configure(EntityTypeBuilder<SpecimenType> builder)
    {
        builder.ToTable("specimen_types");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(s => s.Code).IsRequired().HasMaxLength(100);
        builder.Property(s => s.Name).IsRequired().HasMaxLength(150);
        builder.Property(s => s.Description).IsRequired().HasMaxLength(500);
        builder.HasIndex(s => new { s.OrganizationId, s.Code }).IsUnique();
        builder.HasOne<Organization>()
            .WithMany()
            .HasForeignKey(s => s.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Property(s => s.Version).IsRowVersion();
    }
}

internal sealed class SpecimenSourceConfiguration : IEntityTypeConfiguration<SpecimenSource>
{
    public void Configure(EntityTypeBuilder<SpecimenSource> builder)
    {
        builder.ToTable("specimen_sources");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(s => s.Code).IsRequired().HasMaxLength(100);
        builder.Property(s => s.Name).IsRequired().HasMaxLength(150);
        builder.Property(s => s.Description).IsRequired().HasMaxLength(500);
        builder.HasIndex(s => new { s.OrganizationId, s.Code }).IsUnique();
        builder.HasOne<Organization>()
            .WithMany()
            .HasForeignKey(s => s.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Property(s => s.Version).IsRowVersion();
    }
}

internal sealed class ContainerTypeConfiguration : IEntityTypeConfiguration<ContainerType>
{
    public void Configure(EntityTypeBuilder<ContainerType> builder)
    {
        builder.ToTable("container_types");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(c => c.Code).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Name).IsRequired().HasMaxLength(150);
        builder.Property(c => c.Description).IsRequired().HasMaxLength(500);
        builder.Property(c => c.HandlingNotes).IsRequired().HasMaxLength(500);
        builder.HasIndex(c => new { c.OrganizationId, c.Code }).IsUnique();
        builder.HasOne<Organization>()
            .WithMany()
            .HasForeignKey(c => c.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Property(c => c.Version).IsRowVersion();
    }
}

internal sealed class ResultStatusConfiguration : IEntityTypeConfiguration<ResultStatus>
{
    public void Configure(EntityTypeBuilder<ResultStatus> builder)
    {
        builder.ToTable("result_statuses");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(r => r.Code).IsRequired().HasMaxLength(100);
        builder.Property(r => r.Name).IsRequired().HasMaxLength(150);
        builder.Property(r => r.Description).IsRequired().HasMaxLength(500);
        builder.HasIndex(r => new { r.OrganizationId, r.Code }).IsUnique();
        builder.HasOne<Organization>()
            .WithMany()
            .HasForeignKey(r => r.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Property(r => r.Version).IsRowVersion();
    }
}

internal sealed class AnalyzerConfiguration : IEntityTypeConfiguration<Analyzer>
{
    public void Configure(EntityTypeBuilder<Analyzer> builder)
    {
        builder.ToTable("analyzers");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(a => a.Code).IsRequired().HasMaxLength(100);
        builder.Property(a => a.Name).IsRequired().HasMaxLength(150);
        builder.Property(a => a.Manufacturer).IsRequired().HasMaxLength(150);
        builder.Property(a => a.Model).IsRequired().HasMaxLength(150);
        builder.Property(a => a.Description).IsRequired().HasMaxLength(500);
        builder.HasIndex(a => new { a.OrganizationId, a.FacilityId, a.Code }).IsUnique();
        builder.HasOne<Organization>()
            .WithMany()
            .HasForeignKey(a => a.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Facility>()
            .WithMany()
            .HasForeignKey(a => a.FacilityId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Property(a => a.Version).IsRowVersion();
    }
}

internal sealed class ConfigurationItemConfiguration : IEntityTypeConfiguration<ConfigurationItem>
{
    public void Configure(EntityTypeBuilder<ConfigurationItem> builder)
    {
        builder.ToTable("configuration_items");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(c => c.Key).IsRequired().HasMaxLength(128);
        builder.Property(c => c.Value).IsRequired().HasColumnType("text");
        builder.Property(c => c.Description).IsRequired().HasMaxLength(500);
        builder.HasIndex(c => new { c.OrganizationId, c.Key })
            .IsUnique()
            .HasFilter("\"facility_id\" IS NULL");
        builder.HasIndex(c => new { c.OrganizationId, c.FacilityId, c.Key })
            .IsUnique()
            .HasFilter("\"facility_id\" IS NOT NULL");
        builder.HasOne<Organization>()
            .WithMany()
            .HasForeignKey(c => c.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Facility>()
            .WithMany()
            .HasForeignKey(c => c.FacilityId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Property(c => c.Version).IsRowVersion();
    }
}

internal sealed class TestConfiguration : IEntityTypeConfiguration<Test>
{
    public void Configure(EntityTypeBuilder<Test> builder)
    {
        builder.ToTable("tests");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(t => t.Code).IsRequired().HasMaxLength(100);
        builder.HasIndex(t => new { t.OrganizationId, t.Code }).IsUnique();
        builder.HasOne<Organization>()
            .WithMany()
            .HasForeignKey(t => t.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Property(t => t.Version).IsRowVersion();
    }
}

internal sealed class TestVersionConfiguration : IEntityTypeConfiguration<TestVersion>
{
    public void Configure(EntityTypeBuilder<TestVersion> builder)
    {
        builder.ToTable("test_versions");
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(v => v.Name).IsRequired().HasMaxLength(150);
        builder.Property(v => v.Description).IsRequired().HasMaxLength(500);
        builder.HasIndex(v => new { v.TestId, v.VersionNumber }).IsUnique();
        builder.HasIndex(v => new { v.TestId, v.EffectiveFromUtc });
        builder.HasOne<Test>()
            .WithMany()
            .HasForeignKey(v => v.TestId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Discipline>()
            .WithMany()
            .HasForeignKey(v => v.DisciplineId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ResultDataType>()
            .WithMany()
            .HasForeignKey(v => v.ResultDataTypeId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Unit>()
            .WithMany()
            .HasForeignKey(v => v.UnitId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class TestSpecimenRequirementConfiguration
    : IEntityTypeConfiguration<TestSpecimenRequirement>
{
    public void Configure(EntityTypeBuilder<TestSpecimenRequirement> builder)
    {
        builder.ToTable("test_specimen_requirements");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(r => r.RequiredQuantity).HasPrecision(18, 6);
        builder.Property(r => r.QuantityUnitUcumCode).HasMaxLength(80);
        builder.Property(r => r.Notes).IsRequired().HasMaxLength(500);
        builder.HasIndex(r => new { r.TestId, r.SpecimenTypeId }).IsUnique();
        builder.HasOne<Test>()
            .WithMany()
            .HasForeignKey(r => r.TestId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<SpecimenType>()
            .WithMany()
            .HasForeignKey(r => r.SpecimenTypeId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<SpecimenSource>()
            .WithMany()
            .HasForeignKey(r => r.SpecimenSourceId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ContainerType>()
            .WithMany()
            .HasForeignKey(r => r.ContainerTypeId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Property(r => r.Version).IsRowVersion();
    }
}

internal sealed class TestTerminologyMappingConfiguration
    : IEntityTypeConfiguration<TestTerminologyMapping>
{
    public void Configure(EntityTypeBuilder<TestTerminologyMapping> builder)
    {
        builder.ToTable("test_terminology_mappings");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(m => m.ExternalCode).IsRequired().HasMaxLength(100);
        builder.Property(m => m.ExternalVersion).IsRequired().HasMaxLength(100);
        builder.Property(m => m.Notes).IsRequired().HasMaxLength(500);
        builder.HasIndex(m => new { m.TestId, m.CodeSystemId, m.ExternalCode }).IsUnique();
        builder.HasOne<Test>()
            .WithMany()
            .HasForeignKey(m => m.TestId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<CodeSystem>()
            .WithMany()
            .HasForeignKey(m => m.CodeSystemId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Property(m => m.Version).IsRowVersion();
    }
}

internal sealed class TestFacilityConfiguration : IEntityTypeConfiguration<TestFacility>
{
    public void Configure(EntityTypeBuilder<TestFacility> builder)
    {
        builder.ToTable("test_facilities");
        builder.HasKey(f => f.Id);
        builder.HasIndex(f => new { f.TestId, f.FacilityId }).IsUnique();
        builder.HasOne<Test>()
            .WithMany()
            .HasForeignKey(f => f.TestId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Facility>()
            .WithMany()
            .HasForeignKey(f => f.FacilityId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class PanelConfiguration : IEntityTypeConfiguration<Panel>
{
    public void Configure(EntityTypeBuilder<Panel> builder)
    {
        builder.ToTable("panels");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(p => p.Code).IsRequired().HasMaxLength(100);
        builder.HasIndex(p => new { p.OrganizationId, p.Code }).IsUnique();
        builder.HasOne<Organization>()
            .WithMany()
            .HasForeignKey(p => p.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Property(p => p.Version).IsRowVersion();
    }
}

internal sealed class PanelVersionConfiguration : IEntityTypeConfiguration<PanelVersion>
{
    public void Configure(EntityTypeBuilder<PanelVersion> builder)
    {
        builder.ToTable("panel_versions");
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(v => v.Name).IsRequired().HasMaxLength(150);
        builder.Property(v => v.Description).IsRequired().HasMaxLength(500);
        builder.HasIndex(v => new { v.PanelId, v.VersionNumber }).IsUnique();
        builder.HasOne<Panel>()
            .WithMany()
            .HasForeignKey(v => v.PanelId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class PanelTestMembershipConfiguration : IEntityTypeConfiguration<PanelTestMembership>
{
    public void Configure(EntityTypeBuilder<PanelTestMembership> builder)
    {
        builder.ToTable("panel_test_memberships");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(m => m.DisplayText).IsRequired().HasMaxLength(150);
        builder.HasIndex(m => new { m.PanelId, m.TestId }).IsUnique();
        builder.HasIndex(m => new { m.PanelId, m.SequenceNumber });
        builder.HasOne<Panel>()
            .WithMany()
            .HasForeignKey(m => m.PanelId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Test>()
            .WithMany()
            .HasForeignKey(m => m.TestId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Property(m => m.Version).IsRowVersion();
    }
}

internal sealed class ReferenceRangeConfiguration : IEntityTypeConfiguration<ReferenceRange>
{
    public void Configure(EntityTypeBuilder<ReferenceRange> builder)
    {
        builder.ToTable("reference_ranges");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(r => r.Source).IsRequired().HasMaxLength(300);
        builder.Property(r => r.Population).IsRequired().HasMaxLength(200);
        builder.Property(r => r.SpecimenContext).IsRequired().HasMaxLength(200);
        builder.Property(r => r.TextualReference).IsRequired().HasMaxLength(500);
        builder.Property(r => r.AgeMinimum).HasPrecision(18, 6);
        builder.Property(r => r.AgeMaximum).HasPrecision(18, 6);
        builder.Property(r => r.AgeUnitUcumCode).HasMaxLength(80);
        builder.Property(r => r.LowValue).HasPrecision(18, 6);
        builder.Property(r => r.HighValue).HasPrecision(18, 6);
        builder.HasIndex(r => new { r.OrganizationId, r.TestId });
        builder.HasIndex(r => new { r.TestId, r.EffectiveFromUtc });
        builder.HasOne<Organization>()
            .WithMany()
            .HasForeignKey(r => r.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Facility>()
            .WithMany()
            .HasForeignKey(r => r.FacilityId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Test>()
            .WithMany()
            .HasForeignKey(r => r.TestId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Unit>()
            .WithMany()
            .HasForeignKey(r => r.UnitId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Property(r => r.Version).IsRowVersion();
    }
}
