using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Swasthya.CoreLabs.Domain.Orders;
using Swasthya.CoreLabs.Domain.Specimen;

namespace Swasthya.CoreLabs.Infrastructure.Persistence.Configurations;

internal sealed class SpecimenRequirementConfiguration : IEntityTypeConfiguration<SpecimenRequirement>
{
    public void Configure(EntityTypeBuilder<SpecimenRequirement> builder)
    {
        builder.ToTable("specimen_requirements");

        builder.HasKey(sr => sr.Id);

        builder.Property(sr => sr.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()")
            .ValueGeneratedOnAdd();

        builder.Property(sr => sr.OrderItemId)
            .HasColumnName("order_item_id")
            .IsRequired();

        builder.Property(sr => sr.SpecimenId)
            .HasColumnName("specimen_id")
            .IsRequired();

        builder.Property(sr => sr.SequenceNumber)
            .HasColumnName("sequence_number")
            .IsRequired();

        builder.Property(sr => sr.SpecimenType)
            .HasColumnName("specimen_type")
            .IsRequired();

        builder.Property(sr => sr.Description)
            .HasColumnName("description")
            .HasMaxLength(255)
            .IsRequired(false);

        builder.Property(sr => sr.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(sr => sr.UpdatedAtUtc)
            .HasColumnName("updated_at_utc")
            .IsRequired();

        builder.HasIndex(sr => sr.OrderItemId)
            .HasDatabaseName("ix_specimen_requirements_order_item_id");

        builder.HasIndex(sr => sr.SpecimenId)
            .HasDatabaseName("ix_specimen_requirements_specimen_id");

        builder.HasOne<OrderItem>()
            .WithMany()
            .HasForeignKey(sr => sr.OrderItemId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_specimen_requirements_order_item");

        builder.HasOne<Specimen>()
            .WithMany()
            .HasForeignKey(sr => sr.SpecimenId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_specimen_requirements_specimen");
    }
}
