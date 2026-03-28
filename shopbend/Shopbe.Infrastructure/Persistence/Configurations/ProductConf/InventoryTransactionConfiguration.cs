using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shopbe.Domain.Entities.Product;

namespace Shopbe.Infrastructure.Persistence.Configurations.ProductConf;

public class InventoryTransactionConfiguration : IEntityTypeConfiguration<InventoryTransaction>
{
    public void Configure(EntityTypeBuilder<InventoryTransaction> builder)
    {
        builder.ToTable("InventoryTransactions");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(t => t.UpdatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(t => t.ProductVariantId)
            .IsRequired();

        builder.Property(t => t.Type)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(t => t.Quantity)
            .IsRequired();

        builder.Property(t => t.Note)
            .HasMaxLength(500);

        builder.HasIndex(t => t.ProductVariantId);
        builder.HasIndex(t => new { t.ProductVariantId, t.CreatedAt });

        builder.HasOne(t => t.ProductVariant)
            .WithMany(v => v.InventoryTransactions)
            .HasForeignKey(t => t.ProductVariantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

