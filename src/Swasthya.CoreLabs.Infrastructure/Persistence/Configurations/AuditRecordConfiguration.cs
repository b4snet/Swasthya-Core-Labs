using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Swasthya.CoreLabs.Domain.Audit;
using Swasthya.CoreLabs.Domain.IdentityTenancy;

namespace Swasthya.CoreLabs.Infrastructure.Persistence;

internal sealed class AuditRecordConfiguration : IEntityTypeConfiguration<AuditRecord>
{
    public void Configure(EntityTypeBuilder<AuditRecord> builder)
    {
        builder.ToTable("audit_records");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(a => a.Action).IsRequired().HasMaxLength(100);
        builder.Property(a => a.ResourceType).IsRequired().HasMaxLength(100);
        builder.Property(a => a.ResourceId).HasMaxLength(100);
        builder.Property(a => a.CorrelationId).IsRequired().HasMaxLength(100);
        builder.Property(a => a.ContextMetadata)
            .HasColumnType("jsonb")
            .HasConversion(
                new ValueConverter<string?, string?>(
                    v => string.IsNullOrEmpty(v) ? null : v,
                    v => v));
        builder.HasIndex(a => new { a.OrganizationId, a.OccurredAtUtc });
        builder.HasIndex(a => a.CorrelationId);
        builder.HasOne<Organization>()
            .WithMany()
            .HasForeignKey(a => a.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Facility>()
            .WithMany()
            .HasForeignKey(a => a.FacilityId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
