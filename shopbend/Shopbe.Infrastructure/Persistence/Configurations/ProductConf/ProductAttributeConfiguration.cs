using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductAttributeEntity = Shopbe.Domain.Entities.Product.Attribute;

namespace Shopbe.Infrastructure.Persistence.Configurations.ProductConf;

public class ProductAttributeConfiguration : IEntityTypeConfiguration<ProductAttributeEntity>
{
    public void Configure(EntityTypeBuilder<ProductAttributeEntity> builder)
    {
        builder.ToTable("Attributes");

        builder.HasKey(a => a.Id);

        builder.Property<DateTime>(a => a.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property<DateTime>(a => a.UpdatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.HasIndex(a => a.Name)
            .IsUnique();

        builder.HasMany(a => a.AttributeValues)
            .WithOne(v => v.Attribute)
            .HasForeignKey(v => v.AttributeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

