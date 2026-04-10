using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shopbe.Domain.Entities.Shipping;

namespace Shopbe.Infrastructure.Persistence.Configurations.ShipConf;

public class ShippingZoneDistrictConfiguration : IEntityTypeConfiguration<ShippingZoneDistrict>
{
    public void Configure(EntityTypeBuilder<ShippingZoneDistrict> builder)
    {
        builder.ToTable("ShippingZoneDistricts");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.ZoneId)
            .IsRequired();

        builder.Property(d => d.City)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(d => d.District)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(d => d.ZoneId);
        builder.HasIndex(d => new { d.City, d.District });

        // prevent duplicates per-zone
        builder.HasIndex(d => new { d.ZoneId, d.City, d.District })
            .IsUnique();
    }
}

