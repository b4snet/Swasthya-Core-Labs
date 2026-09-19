using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Swasthya.CoreLabs.Domain.IdentityTenancy;
using Swasthya.CoreLabs.Domain.Specimen;

namespace Swasthya.CoreLabs.Infrastructure.Persistence.Configurations;

internal sealed class AccessionConfiguration : IEntityTypeConfiguration<Accession>
{
    public void Configure(EntityTypeBuilder<Accession> builder)
    {
        builder.ToTable("accessions");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()")
            .ValueGeneratedOnAdd();

        builder.Property(a => a.FacilityId)
            .HasColumnName("facility_id")
            .IsRequired();

        builder.Property(a => a.AccessionNumber)
            .HasColumnName("accession_number")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(a => a.Description)
            .HasColumnName("description")
            .HasMaxLength(255)
            .IsRequired(false);

        builder.Property(a => a.ReceivingActorId)
            .HasColumnName("receiving_actor_id")
            .IsRequired(false);

        builder.Property(a => a.Status)
            .HasColumnName("status")
            .IsRequired();

        builder.Property(a => a.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(a => a.UpdatedAtUtc)
            .HasColumnName("updated_at_utc")
            .IsRequired();

        builder.HasIndex(a => a.FacilityId)
            .HasDatabaseName("ix_accessions_facility_id");

        builder.HasIndex(a => a.AccessionNumber)
            .HasDatabaseName("ix_accessions_accession_number")
            .IsUnique();

        builder.ToTable("accessions", t =>
        {
            t.HasCheckConstraint("ck_accessions_status_valid", "status IN (0, 1, 2, 3, 4)");
        });

        builder.HasOne<Facility>()
            .WithMany()
            .HasForeignKey(a => a.FacilityId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_accessions_facility");
    }
}
