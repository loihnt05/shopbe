using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shopbe.Domain.Entities.Product;

namespace Shopbe.Infrastructure.Persistence.Configurations.ProductConf;

public class AttributeValueConfiguration : IEntityTypeConfiguration<AttributeValue>
{
    public void Configure(EntityTypeBuilder<AttributeValue> builder)
    {
        builder.ToTable("AttributeValues");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(v => v.UpdatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(v => v.ProductAttributeId)
            .IsRequired();

        builder.Property(v => v.Value)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(v => v.ProductAttributeId);
        builder.HasIndex(v => new { v.ProductAttributeId, v.Value })
            .IsUnique();

        builder.HasMany(v => v.ProductVariantAttributes)
            .WithOne(pva => pva.AttributeValue)
            .HasForeignKey(pva => pva.AttributeValueId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

