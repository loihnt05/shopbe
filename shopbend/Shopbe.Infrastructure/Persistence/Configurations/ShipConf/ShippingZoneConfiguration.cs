using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shopbe.Domain.Entities.Shipping;

namespace Shopbe.Infrastructure.Persistence.Configurations.ShipConf;

public class ShippingZoneConfiguration : IEntityTypeConfiguration<ShippingZone>
{
    public void Configure(EntityTypeBuilder<ShippingZone> builder)
    {
        builder.ToTable("ShippingZones");

        builder.HasKey(z => z.Id);

        builder.Property(z => z.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(z => z.Fee)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.HasIndex(z => z.Name)
            .IsUnique();

        builder.HasMany(z => z.ShippingZoneDistricts)
            .WithOne(d => d.Zone)
            .HasForeignKey(d => d.ZoneId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

