using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shopbe.Domain.Entities.Category;
using Shopbe.Domain.Entities.Order;
using Shopbe.Domain.Entities.Recommendation;

namespace Shopbe.Infrastructure.Persistence.Configurations.RecommendationConf;

public class UserBehaviorConfiguration : IEntityTypeConfiguration<UserBehavior>
{
    public void Configure(EntityTypeBuilder<UserBehavior> builder)
    {
        builder.ToTable("UserBehaviors");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ActionType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.SessionId)
            .HasMaxLength(128);

        builder.Property(x => x.CorrelationId)
            .HasMaxLength(128);

        builder.Property(x => x.BehaviorType)
            .IsRequired();

        builder.Property(x => x.OccurredAt)
            .IsRequired();

        builder.Property(x => x.ExpiresAt)
            .IsRequired();

        builder.Property(x => x.Referrer)
            .HasMaxLength(2048);

        builder.Property(x => x.Source)
            .HasMaxLength(64);

        builder.Property(x => x.Device)
            .HasMaxLength(64);

        builder.Property(x => x.Currency)
            .HasMaxLength(3);

        builder.Property(x => x.UserAgent)
            .HasMaxLength(512);

        builder.Property(x => x.IpAddress)
            .HasMaxLength(64);

        builder.Property(x => x.Country)
            .HasMaxLength(2);

        builder.Property(x => x.City)
            .HasMaxLength(128);

        builder.Property(x => x.Metadata)
            .HasColumnType("text");

        // Indexes to support typical recommendation queries
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.SessionId);
        builder.HasIndex(x => x.CorrelationId);
        builder.HasIndex(x => x.ProductId);
        builder.HasIndex(x => x.CategoryId);
        builder.HasIndex(x => x.OrderId);
        builder.HasIndex(x => x.OccurredAt);
        builder.HasIndex(x => x.ExpiresAt);
        builder.HasIndex(x => new { x.UserId, x.OccurredAt });
        builder.HasIndex(x => new { x.ProductId, x.OccurredAt });
        builder.HasIndex(x => new { x.UserId, x.ProductId, x.BehaviorType });
        builder.HasIndex(x => new { x.UserId, x.CategoryId, x.BehaviorType });

        // Relationships
        builder.HasOne(x => x.User)
            .WithMany(u => u.UserBehaviors)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Product)
            .WithMany(p => p.UserBehaviors)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne<Order>()
            .WithMany()
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}


