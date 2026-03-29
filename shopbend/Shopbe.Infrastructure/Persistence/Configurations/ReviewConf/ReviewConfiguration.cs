using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shopbe.Domain.Entities.Review;

namespace Shopbe.Infrastructure.Persistence.Configurations.ReviewConf;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable("Reviews");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.UserId)
            .IsRequired();

        builder.Property(r => r.ProductId)
            .IsRequired();

        builder.Property(r => r.OrderId)
            .IsRequired();

        builder.Property(r => r.Rating)
            .IsRequired();

        builder.Property(r => r.Comment)
            .HasMaxLength(2000);

        builder.Property(r => r.IsVisible)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasIndex(r => r.UserId);
        builder.HasIndex(r => r.ProductId);
        builder.HasIndex(r => r.OrderId);
        builder.HasIndex(r => new { r.ProductId, r.UserId });

        // Relationships
        builder.HasOne(r => r.User)
            .WithMany(u => u.Reviews)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Product)
            .WithMany(p => p.Reviews)
            .HasForeignKey(r => r.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Order)
            .WithMany()
            .HasForeignKey(r => r.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(r => r.ReviewImages)
            .WithOne(ri => ri.Review)
            .HasForeignKey(ri => ri.ReviewId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}