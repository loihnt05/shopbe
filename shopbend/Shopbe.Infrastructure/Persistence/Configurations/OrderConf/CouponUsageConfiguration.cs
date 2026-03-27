using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shopbe.Domain.Entities.Order;

namespace Shopbe.Infrastructure.Persistence.Configurations.OrderConf;

public class CouponUsageConfiguration : IEntityTypeConfiguration<CouponUsage>
{
    public void Configure(EntityTypeBuilder<CouponUsage> builder)
    {
        builder.ToTable("CouponUsages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(x => x.UpdatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(x => x.CouponId)
            .IsRequired();

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.HasIndex(x => x.CouponId);
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.OrderId);
        builder.HasIndex(x => new { x.CouponId, x.UserId, x.OrderId });

        builder.HasOne(x => x.Coupon)
            .WithMany(c => c.CouponUsages)
            .HasForeignKey(x => x.CouponId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.User)
            .WithMany(u => u.CouponUsages)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Order)
            .WithMany(o => o.CouponUsages)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

