using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Swasthya.CoreLabs.Domain.IdentityTenancy;
using Swasthya.CoreLabs.Domain.Laboratory;
using Swasthya.CoreLabs.Domain.Orders;

namespace Swasthya.CoreLabs.Infrastructure.Persistence;

internal sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(o => o.OrderNumber).IsRequired().HasMaxLength(100);
        builder.Property(o => o.ExternalOrderId).HasMaxLength(200);
        builder.Property(o => o.PatientExternalSystem).IsRequired().HasMaxLength(100);
        builder.Property(o => o.PatientExternalIdentifier).IsRequired().HasMaxLength(200);
        builder.Property(o => o.EncounterExternalSystem).HasMaxLength(100);
        builder.Property(o => o.EncounterExternalIdentifier).HasMaxLength(200);
        builder.HasIndex(o => new { o.OrganizationId, o.OrderNumber }).IsUnique();
        builder.HasIndex(o => new { o.OrganizationId, o.ExternalOrderId })
            .IsUnique()
            .HasFilter("\"external_order_id\" IS NOT NULL");
        builder.HasIndex(o => new { o.OrganizationId, o.FacilityId, o.Status });
        builder.HasIndex(o => new { o.OrganizationId, o.Status, o.RequestedAtUtc });
        builder.HasOne<Organization>()
            .WithMany()
            .HasForeignKey(o => o.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Facility>()
            .WithMany()
            .HasForeignKey(o => o.FacilityId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Principal>()
            .WithMany()
            .HasForeignKey(o => o.RequestedByPrincipalId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Property(o => o.Version).IsRowVersion();
    }
}

internal sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable(
            "order_items",
            table => table.HasCheckConstraint(
                "ck_order_items_single_target",
                "((test_id IS NOT NULL)::int + (panel_id IS NOT NULL)::int) = 1"));
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(i => i.TestCode).HasMaxLength(100);
        builder.Property(i => i.TestName).HasMaxLength(150);
        builder.Property(i => i.PanelCode).HasMaxLength(100);
        builder.Property(i => i.PanelName).HasMaxLength(150);
        builder.Property(i => i.RequestedQuantity).HasPrecision(18, 6);
        builder.Property(i => i.QuantityUnitUcumCode).HasMaxLength(80);
        builder.HasIndex(i => new { i.OrderId, i.SequenceNumber }).IsUnique();
        builder.HasIndex(i => new { i.OrderId, i.Status });
        builder.HasOne<Order>()
            .WithMany()
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Test>()
            .WithMany()
            .HasForeignKey(i => i.TestId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<TestVersion>()
            .WithMany()
            .HasForeignKey(i => i.TestVersionId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Panel>()
            .WithMany()
            .HasForeignKey(i => i.PanelId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<PanelVersion>()
            .WithMany()
            .HasForeignKey(i => i.PanelVersionId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Property(i => i.Version).IsRowVersion();
    }
}
