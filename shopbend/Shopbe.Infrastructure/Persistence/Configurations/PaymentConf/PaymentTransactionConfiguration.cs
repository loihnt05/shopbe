using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shopbe.Domain.Entities.Payment;

namespace Shopbe.Infrastructure.Persistence.Configurations.PaymentConf;

public class PaymentTransactionConfiguration : IEntityTypeConfiguration<PaymentTransaction>
{
    public void Configure(EntityTypeBuilder<PaymentTransaction> builder)
    {
        builder.ToTable("PaymentTransactions");

        builder.HasKey(pt => pt.Id);

        builder.Property(pt => pt.PaymentId)
            .IsRequired();

        builder.Property(pt => pt.ExternalTransactionId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(pt => pt.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(pt => pt.Amount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(pt => pt.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(pt => pt.UpdatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(pt => pt.PaymentId);
        builder.HasIndex(pt => pt.ExternalTransactionId);

        builder.HasOne(pt => pt.Payment)
            .WithMany(p => p.PaymentTransactions)
            .HasForeignKey(pt => pt.PaymentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

