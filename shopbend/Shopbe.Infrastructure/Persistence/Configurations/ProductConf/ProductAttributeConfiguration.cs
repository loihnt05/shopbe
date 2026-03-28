using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shopbe.Domain.Entities.Product;

namespace Shopbe.Infrastructure.Persistence.Configurations.ProductConf;

public class ProductAttributeConfiguration : IEntityTypeConfiguration<ProductAttribute>
{
    public void Configure(EntityTypeBuilder<ProductAttribute> builder)
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

