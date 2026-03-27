using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shopbe.Domain.Entities.Product;

namespace Shopbe.Infrastructure.Persistence.Configurations.ProductConf;

public class ProductVariantAttributeConfiguration : IEntityTypeConfiguration<ProductVariantAttribute>
{
    public void Configure(EntityTypeBuilder<ProductVariantAttribute> builder)
    {
        builder.ToTable("ProductVariantAttributes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(x => x.UpdatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(x => x.ProductVariantId)
            .IsRequired();

        builder.Property(x => x.AttributeValueId)
            .IsRequired();

        builder.HasIndex(x => x.ProductVariantId);
        builder.HasIndex(x => x.AttributeValueId);
        builder.HasIndex(x => new { x.ProductVariantId, x.AttributeValueId })
            .IsUnique();

        builder.HasOne(x => x.ProductVariant)
            .WithMany(v => v.ProductVariantAttributes)
            .HasForeignKey(x => x.ProductVariantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.AttributeValue)
            .WithMany(v => v.ProductVariantAttributes)
            .HasForeignKey(x => x.AttributeValueId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

