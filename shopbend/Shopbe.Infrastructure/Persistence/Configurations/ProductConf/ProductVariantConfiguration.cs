using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shopbe.Domain.Entities.Product;

namespace Shopbe.Infrastructure.Persistence.Configurations.ProductConf;

public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.ToTable("ProductVariants");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(v => v.UpdatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(v => v.Sku)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(v => v.Price)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(v => v.StockQuantity)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(v => v.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(v => v.DeletedAt)
            .IsRequired(false);

        builder.HasIndex(v => v.DeletedAt);

        builder.Property(v => v.ProductId)
            .IsRequired();

        builder.HasIndex(v => v.ProductId);
        builder.HasIndex(v => new { v.ProductId, v.Sku }).IsUnique();

        builder.HasMany(v => v.ProductVariantAttributes)
            .WithOne(pva => pva.Variant)
            .HasForeignKey(pva => pva.VariantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(v => v.InventoryTransactions)
            .WithOne(t => t.ProductVariant)
            .HasForeignKey(t => t.ProductVariantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
