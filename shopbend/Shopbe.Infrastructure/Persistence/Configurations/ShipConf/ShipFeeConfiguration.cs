using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shopbe.Domain.Entities.Shipping;

namespace Shopbe.Infrastructure.Persistence.Configurations.ShipConf;

public class ShipFeeConfiguration : IEntityTypeConfiguration<ShippingFee>
{
    public void Configure(EntityTypeBuilder<ShippingFee> builder)
    {
        builder.ToTable("ShippingFees");

        builder.HasKey(sf => sf.Id);

        builder.Property(sf => sf.Region)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(sf => sf.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(sf => sf.Currency)
            .IsRequired()
            .HasMaxLength(3)
            .HasDefaultValue("USD");

        builder.HasIndex(sf => sf.Region);

        builder.HasMany(sf => sf.Shipments)
            .WithOne(s => s.ShippingFee)
            .HasForeignKey(s => s.ShippingFeeId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}