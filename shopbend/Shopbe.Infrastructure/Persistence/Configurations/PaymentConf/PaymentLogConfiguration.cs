using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shopbe.Domain.Entities.Payment;

namespace Shopbe.Infrastructure.Persistence.Configurations.PaymentConf;

public class PaymentLogConfiguration : IEntityTypeConfiguration<PaymentLog>
{
    public void Configure(EntityTypeBuilder<PaymentLog> builder)
    {
        builder.ToTable("PaymentLogs");

        builder.HasKey(pl => pl.Id);

        builder.Property(pl => pl.PaymentId)
            .IsRequired();

        builder.Property(pl => pl.Level)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(pl => pl.Message)
            .IsRequired()
            .HasMaxLength(2000);


        builder.Property(pl => pl.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(pl => pl.UpdatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(pl => pl.PaymentId);
        builder.HasIndex(pl => pl.Level);

        builder.HasOne(pl => pl.Payment)
            .WithMany(p => p.PaymentLogs)
            .HasForeignKey(pl => pl.PaymentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

