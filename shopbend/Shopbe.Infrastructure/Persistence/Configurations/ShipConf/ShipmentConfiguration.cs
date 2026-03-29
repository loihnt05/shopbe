using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shopbe.Domain.Entities.Shipping;

namespace Shopbe.Infrastructure.Persistence.Configurations.ShipConf;

public class ShipmentConfiguration : IEntityTypeConfiguration<Shipment>
{
    public void Configure(EntityTypeBuilder<Shipment> builder)
    {
        builder.ToTable("Shipments");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.OrderId)
            .IsRequired();

        builder.Property(s => s.Carrier)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.TrackingNumber)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.ShippedAt)
            .IsRequired(false);

        builder.Property(s => s.DeliveredAt)
            .IsRequired(false);

        builder.Property(s => s.ShippingFeeId)
            .IsRequired(false);

        builder.HasIndex(s => s.OrderId);
        builder.HasIndex(s => s.ShippingFeeId);
        builder.HasIndex(s => s.TrackingNumber);

        // Relationships
        builder.HasOne(s => s.Order)
            .WithMany(o => o.Shipments)
            .HasForeignKey(s => s.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.ShippingFee)
            .WithMany(sf => sf.Shipments)
            .HasForeignKey(s => s.ShippingFeeId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}