using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shopbe.Domain.Entities.Wishlist;

namespace Shopbe.Infrastructure.Persistence.Configurations.WishConf;

public class WishItemConfiguration : IEntityTypeConfiguration<WishlistItem>
{
    public void Configure(EntityTypeBuilder<WishlistItem> builder)
    {
        builder.ToTable("WishlistItems");

        builder.HasKey(wi => wi.Id);

        builder.Property(wi => wi.UserId)
            .IsRequired();

        builder.Property(wi => wi.ProductId)
            .IsRequired();

        // Prevent a user from wishlisting the same product multiple times
        builder.HasIndex(wi => new { wi.UserId, wi.ProductId })
            .IsUnique();

        // Supporting indexes for common query patterns
        builder.HasIndex(wi => wi.UserId);
        builder.HasIndex(wi => wi.ProductId);

        // Relationships
        builder.HasOne(wi => wi.User)
            .WithMany(u => u.WishlistItems)
            .HasForeignKey(wi => wi.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // If a product gets deleted, we keep history safe by restricting by default.
        // If you prefer automatically removing wishlist entries, switch to Cascade.
        builder.HasOne(wi => wi.Product)
            .WithMany(p => p.WishlistItems)
            .HasForeignKey(wi => wi.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}