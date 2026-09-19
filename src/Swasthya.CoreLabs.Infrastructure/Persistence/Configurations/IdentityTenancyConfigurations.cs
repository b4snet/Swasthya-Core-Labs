using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Swasthya.CoreLabs.Domain.IdentityTenancy;

namespace Swasthya.CoreLabs.Infrastructure.Persistence;

internal sealed class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.ToTable("organizations");
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(o => o.Name).IsRequired().HasMaxLength(150);
        builder.HasIndex(o => o.Name).IsUnique();
        builder.Property(o => o.Version).IsRowVersion();
        builder.HasMany(o => o.Facilities)
            .WithOne(f => f.Organization)
            .HasForeignKey(f => f.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(o => o.Roles)
            .WithOne(r => r.Organization)
            .HasForeignKey(r => r.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class FacilityConfiguration : IEntityTypeConfiguration<Facility>
{
    public void Configure(EntityTypeBuilder<Facility> builder)
    {
        builder.ToTable("facilities");
        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(f => f.Code).IsRequired().HasMaxLength(30);
        builder.Property(f => f.Name).IsRequired().HasMaxLength(150);
        builder.HasIndex(f => new { f.OrganizationId, f.Code }).IsUnique();
        builder.Property(f => f.Version).IsRowVersion();
    }
}

internal sealed class PrincipalConfiguration : IEntityTypeConfiguration<Principal>
{
    public void Configure(EntityTypeBuilder<Principal> builder)
    {
        builder.ToTable("principals");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(p => p.Issuer).IsRequired().HasMaxLength(2048);
        builder.Property(p => p.ExternalSubject).IsRequired().HasMaxLength(255);
        builder.Property(p => p.DisplayName).IsRequired().HasMaxLength(150);
        builder.HasIndex(p => new { p.Issuer, p.ExternalSubject }).IsUnique();
        builder.Property(p => p.Version).IsRowVersion();
    }
}

internal sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("permissions");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(p => p.Code).IsRequired().HasMaxLength(100);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(150);
        builder.Property(p => p.Description).HasMaxLength(500);
        builder.HasIndex(p => p.Code).IsUnique();
        builder.Property(p => p.Version).IsRowVersion();
    }
}

internal sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("roles");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(r => r.Code).IsRequired().HasMaxLength(100);
        builder.Property(r => r.Name).IsRequired().HasMaxLength(150);
        builder.Property(r => r.Description).HasMaxLength(500);
        builder.HasIndex(r => new { r.OrganizationId, r.Code }).IsUnique();
        builder.Property(r => r.Version).IsRowVersion();
        builder.HasMany(r => r.RolePermissions)
            .WithOne(rp => rp.Role)
            .HasForeignKey(rp => rp.RoleId);
    }
}

internal sealed class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("role_permissions");
        builder.HasKey(rp => rp.Id);
        builder.Property(rp => rp.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.HasIndex(rp => new { rp.RoleId, rp.PermissionId }).IsUnique();
        builder.HasOne(rp => rp.Permission)
            .WithMany()
            .HasForeignKey(rp => rp.PermissionId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Property(rp => rp.Version).IsRowVersion();
    }
}

internal sealed class PrincipalRoleAssignmentConfiguration : IEntityTypeConfiguration<PrincipalRoleAssignment>
{
    public void Configure(EntityTypeBuilder<PrincipalRoleAssignment> builder)
    {
        builder.ToTable("principal_role_assignments");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.HasIndex(a => new { a.PrincipalId, a.OrganizationId })
            .IsUnique()
            .HasFilter("\"facility_id\" IS NULL");
        builder.HasIndex(a => new { a.PrincipalId, a.OrganizationId, a.FacilityId })
            .IsUnique()
            .HasFilter("\"facility_id\" IS NOT NULL");
        builder.HasOne(a => a.Role)
            .WithMany()
            .HasForeignKey(a => a.RoleId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(a => a.Organization)
            .WithMany()
            .HasForeignKey(a => a.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(a => a.Facility)
            .WithMany()
            .HasForeignKey(a => a.FacilityId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Property(a => a.Version).IsRowVersion();
    }
}
