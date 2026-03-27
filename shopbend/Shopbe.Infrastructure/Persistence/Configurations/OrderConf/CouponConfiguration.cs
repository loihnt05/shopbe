using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shopbe.Domain.Entities.Order;

namespace Shopbe.Infrastructure.Persistence.Configurations.OrderConf;

public class CouponConfiguration : IEntityTypeConfiguration<Coupon>
{
    public void Configure(EntityTypeBuilder<Coupon> builder)
    {
        builder.ToTable("Coupons");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(c => c.UpdatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(c => c.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.DiscountValue)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(c => c.IsPercentage)
            .IsRequired();

        builder.Property(c => c.MaxUsageCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.HasIndex(c => c.Code)
            .IsUnique();

        builder.HasMany(c => c.Orders)
            .WithOne(o => o.Coupon)
            .HasForeignKey(o => o.CouponId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(c => c.CouponUsages)
            .WithOne(cu => cu.Coupon)
            .HasForeignKey(cu => cu.CouponId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

