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

        builder.Property(c => c.Description)
            .HasMaxLength(500);

        builder.Property(c => c.DiscountType)
            .IsRequired();

        builder.Property(c => c.Value)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(c => c.MinOrderAmount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(c => c.MaxDiscountAmount)
            .HasColumnType("decimal(18,2)");

        builder.Property(c => c.ExpiredAt)
            .IsRequired();

        builder.Property(c => c.UsageLimit);

        builder.Property(c => c.UsageCount)
            .IsRequired();

        builder.Property(c => c.IsActive)
            .IsRequired();

        builder.HasIndex(c => c.Code)
            .IsUnique();


        builder.HasMany(c => c.CouponUsages)
            .WithOne(cu => cu.Coupon)
            .HasForeignKey(cu => cu.CouponId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

