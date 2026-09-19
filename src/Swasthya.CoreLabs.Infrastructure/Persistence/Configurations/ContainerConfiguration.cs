using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Swasthya.CoreLabs.Domain.IdentityTenancy;
using Swasthya.CoreLabs.Domain.Orders;
using Swasthya.CoreLabs.Domain.Specimen;

namespace Swasthya.CoreLabs.Infrastructure.Persistence.Configurations;

internal sealed class ContainerConfiguration : IEntityTypeConfiguration<Container>
{
    public void Configure(EntityTypeBuilder<Container> builder)
    {
        builder.ToTable("containers");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()")
            .ValueGeneratedOnAdd();

        builder.Property(c => c.FacilityId)
            .HasColumnName("facility_id")
            .IsRequired();

        builder.Property(c => c.SpecimenType)
            .HasColumnName("specimen_type")
            .IsRequired();

        builder.Property(c => c.Label)
            .HasColumnName("label")
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(c => c.Barcode)
            .HasColumnName("barcode")
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(c => c.Status)
            .HasColumnName("status")
            .IsRequired();

        builder.Property(c => c.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(c => c.UpdatedAtUtc)
            .HasColumnName("updated_at_utc")
            .IsRequired();

        builder.HasIndex(c => c.FacilityId)
            .HasDatabaseName("ix_containers_facility_id");

        builder.ToTable("containers", t =>
        {
            t.HasCheckConstraint("ck_containers_status_valid", "status IN (0, 1, 2, 3, 4, 5, 6)");
        });

        builder.HasOne<Facility>()
            .WithMany()
            .HasForeignKey(c => c.FacilityId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_containers_facility");
    }
}
