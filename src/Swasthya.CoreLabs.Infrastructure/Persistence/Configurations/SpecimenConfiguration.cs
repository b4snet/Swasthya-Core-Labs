using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Swasthya.CoreLabs.Domain.Orders;
using Swasthya.CoreLabs.Domain.Specimen;

namespace Swasthya.CoreLabs.Infrastructure.Persistence.Configurations;

internal sealed class SpecimenConfiguration : IEntityTypeConfiguration<Specimen>
{
    public void Configure(EntityTypeBuilder<Specimen> builder)
    {
        builder.ToTable("specimens");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()")
            .ValueGeneratedOnAdd();

        builder.Property(s => s.OrderItemId)
            .HasColumnName("order_item_id")
            .IsRequired();

        builder.Property(s => s.ContainerId)
            .HasColumnName("container_id")
            .IsRequired();

        builder.Property(s => s.SpecimenType)
            .HasColumnName("specimen_type")
            .IsRequired();

        builder.Property(s => s.AccessionNumber)
            .HasColumnName("accession_number")
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(s => s.Status)
            .HasColumnName("status")
            .IsRequired();

        builder.Property(s => s.CollectorIdentity)
            .HasColumnName("collector_identity")
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(s => s.FacilityId)
            .HasColumnName("facility_id")
            .IsRequired(false);

        builder.Property(s => s.CollectionNotes)
            .HasColumnName("collection_notes")
            .HasMaxLength(255)
            .IsRequired(false);

        builder.Property(s => s.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(s => s.UpdatedAtUtc)
            .HasColumnName("updated_at_utc")
            .IsRequired();

        builder.HasIndex(s => s.OrderItemId)
            .HasDatabaseName("ix_specimens_order_item_id");

        builder.HasIndex(s => s.ContainerId)
            .HasDatabaseName("ix_specimens_container_id");

        builder.ToTable("specimens", t =>
        {
            t.HasCheckConstraint("ck_specimens_status_valid", "status IN (0, 1, 2, 3, 4, 5, 6)");
        });

        builder.HasOne<OrderItem>()
            .WithMany()
            .HasForeignKey(s => s.OrderItemId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_specimens_order_item");

        builder.HasOne<Container>()
            .WithMany()
            .HasForeignKey(s => s.ContainerId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_specimens_container");
    }
}
