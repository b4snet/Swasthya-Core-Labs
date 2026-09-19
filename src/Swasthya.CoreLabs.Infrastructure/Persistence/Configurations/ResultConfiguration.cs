using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Swasthya.CoreLabs.Domain.Results;

namespace Swasthya.CoreLabs.Infrastructure.Persistence.Configurations;

public class ResultConfiguration : IEntityTypeConfiguration<Result>
{
    public void Configure(EntityTypeBuilder<Result> builder)
    {
        // Table and primary key
        builder.ToTable("results");

        builder.HasKey(r => r.Id);

        // Property mappings (existing domain model, preserved unchanged)
        builder.Property(r => r.Id)
            .HasColumnName("id")
            .HasColumnType("uuid");

        builder.Property(r => r.OrderItemId)
            .HasColumnName("order_item_id")
            .HasColumnType("uuid");

        builder.Property(r => r.SpecimenId)
            .HasColumnName("specimen_id")
            .HasColumnType("uuid");

        builder.Property(r => r.TestVersionId)
            .HasColumnName("test_version_id")
            .HasColumnType("uuid");

        builder.Property(r => r.ValueTypeCode)
            .HasColumnName("value_type_code")
            .HasColumnType("integer");

        // Public domain Version property — preserved as long, mapped to bigint column
        builder.Property<long>("Version")
            .HasColumnName("version")
            .HasColumnType("bigint")
            .IsConcurrencyToken();

        // Shadow uint concurrency property mapped to PostgreSQL xmin system column.
        // This is the actual optimistic concurrency mechanism; it does not change the
        // public Result.Version long type or the existing bigint column.
        builder.Property<uint>("_versionXmin")
            .IsRowVersion();

        // Remaining existing properties (mapped conventionally)
        builder.Property(r => r.NumericValue).HasColumnName("numeric_value").HasColumnType("numeric").IsRequired(false);
        builder.Property(r => r.TextualValue).HasColumnName("textual_value").HasColumnType("text").IsRequired(false);
        builder.Property(r => r.CodedValue).HasColumnName("coded_value").HasColumnType("text").IsRequired(false);
        builder.Property(r => r.CodedSystem).HasColumnName("coded_system").HasColumnType("text").IsRequired(false);
        builder.Property(r => r.UnitUcumCode).HasColumnName("unit_ucum_code").HasColumnType("text").IsRequired(false);
        builder.Property(r => r.ObservedAtUtc).HasColumnName("observed_at_utc").HasColumnType("timestamp with time zone");
        builder.Property(r => r.EnteredAtUtc).HasColumnName("entered_at_utc").HasColumnType("timestamp with time zone");
        builder.Property(r => r.EnteredByPrincipalId).HasColumnName("entered_by_principal_id").HasColumnType("uuid");
        builder.Property(r => r.StatusCode).HasColumnName("status_code").HasColumnType("text").HasDefaultValue("entered");
        builder.Property(r => r.CreatedAtUtc).HasColumnName("created_at_utc").HasColumnType("timestamp with time zone");
        builder.Property(r => r.UpdatedAtUtc).HasColumnName("updated_at_utc").HasColumnType("timestamp with time zone");
    }
}