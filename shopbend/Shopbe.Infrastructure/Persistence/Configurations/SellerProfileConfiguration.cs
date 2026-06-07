using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shopbe.Domain.Entities.Seller;
using Shopbe.Domain.Enums;

namespace Shopbe.Infrastructure.Persistence.Configurations;

public class SellerProfileConfiguration : IEntityTypeConfiguration<SellerProfile>
{
    public void Configure(EntityTypeBuilder<SellerProfile> builder)
    {
        builder.ToTable("SellerProfiles");

        builder.HasKey(sp => sp.Id);

        builder.Property(sp => sp.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(sp => sp.UpdatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(sp => sp.UserId)
            .IsRequired();

        builder.HasIndex(sp => sp.UserId)
            .IsUnique();

        builder.Property(sp => sp.ShopName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(sp => sp.ShopDescription)
            .HasMaxLength(2000);

        builder.Property(sp => sp.ShopLogoUrl)
            .HasMaxLength(2048);

        builder.Property(sp => sp.ShopBannerUrl)
            .HasMaxLength(2048);

        builder.Property(sp => sp.ContactPhone)
            .HasMaxLength(20);

        builder.Property(sp => sp.ContactEmail)
            .HasMaxLength(256);

        builder.Property(sp => sp.Address)
            .HasMaxLength(500);

        builder.Property(sp => sp.City)
            .HasMaxLength(100);

        builder.Property(sp => sp.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasSentinel(SellerStatus.Active)
            .HasDefaultValue(SellerStatus.Pending);

        builder.Property(sp => sp.CommissionRate)
            .HasPrecision(5, 4)
            .HasDefaultValue(0.05m);

        builder.Property(sp => sp.Rating)
            .HasPrecision(3, 2);

        builder.Property(sp => sp.TotalSales)
            .HasDefaultValue(0);

        builder.Property(sp => sp.TotalRevenue)
            .HasPrecision(18, 2)
            .HasDefaultValue(0m);

        builder.HasOne(sp => sp.User)
            .WithOne(u => u.SellerProfile)
            .HasForeignKey<SellerProfile>(sp => sp.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
