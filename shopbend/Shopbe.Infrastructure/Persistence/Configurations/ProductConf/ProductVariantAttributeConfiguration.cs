using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shopbe.Domain.Entities.Product;

namespace Shopbe.Infrastructure.Persistence.Configurations.ProductConf;

public class ProductVariantAttributeConfiguration : IEntityTypeConfiguration<ProductVariantAttribute>
{
    public void Configure(EntityTypeBuilder<ProductVariantAttribute> builder)
    {
        builder.ToTable("ProductVariantAttributes");

        builder.HasKey(x => new { x.VariantId, x.AttributeValueId });

        builder.Property(x => x.VariantId)
            .IsRequired();

        builder.Property(x => x.AttributeValueId)
            .IsRequired();

        builder.HasIndex(x => x.VariantId);
        builder.HasIndex(x => x.AttributeValueId);

        builder.HasOne(x => x.Variant)
            .WithMany(v => v.ProductVariantAttributes)
            .HasForeignKey(x => x.VariantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.AttributeValue)
            .WithMany(av => av.ProductVariantAttributes)
            .HasForeignKey(x => x.AttributeValueId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

