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

        builder.Property(i => i.EntityType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(i => i.EntityId);

        builder.Property(i => i.Response)
            .HasColumnType("text");

        builder.Property(i => i.ExpiresAt)
            .IsRequired();

        builder.Property(i => i.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(i => i.UpdatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(i => new { i.Key, i.EntityType })
            .IsUnique();

        builder.HasIndex(i => i.ExpiresAt);
    }
}

