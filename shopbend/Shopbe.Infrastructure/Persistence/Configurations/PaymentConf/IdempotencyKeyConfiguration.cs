using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shopbe.Domain.Entities.Payment;

namespace Shopbe.Infrastructure.Persistence.Configurations.PaymentConf;

public class IdempotencyKeyConfiguration : IEntityTypeConfiguration<IdempotencyKey>
{
    public void Configure(EntityTypeBuilder<IdempotencyKey> builder)
    {
        builder.ToTable("IdempotencyKeys");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Key)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(i => i.Scope)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(i => i.RequestHash)
            .HasMaxLength(200);

        builder.Property(i => i.ResponseBody)
            .HasColumnType("text");

        builder.Property(i => i.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(i => i.UpdatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(i => new { i.Key, i.Scope })
            .IsUnique();

        builder.HasIndex(i => i.ExpiresAt);
    }
}

