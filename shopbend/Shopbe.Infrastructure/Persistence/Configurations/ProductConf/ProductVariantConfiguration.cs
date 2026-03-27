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

        builder.Property(v => v.SKU)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(v => v.Price)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(v => v.StockQuantity)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(v => v.ImageUrl)
            .HasMaxLength(500);

        builder.Property(v => v.ProductId)
            .IsRequired();

        builder.HasIndex(v => v.ProductId);
        builder.HasIndex(v => new { v.ProductId, v.SKU }).IsUnique();
    }
}
