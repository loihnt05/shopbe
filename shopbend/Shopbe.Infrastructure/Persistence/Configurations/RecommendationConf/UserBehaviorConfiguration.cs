using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
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

        builder.Property(x => x.BehaviorType)
            .IsRequired();

        builder.Property(x => x.OccurredAt)
            .IsRequired();

        builder.Property(x => x.Referrer)
            .HasMaxLength(2048);

        builder.Property(x => x.UserAgent)
            .HasMaxLength(512);

        builder.Property(x => x.IpAddress)
            .HasMaxLength(64);

        builder.Property(x => x.Metadata)
            .HasColumnType("text");

        // Indexes to support typical recommendation queries
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.SessionId);
        builder.HasIndex(x => x.ProductId);
        builder.HasIndex(x => x.OccurredAt);
        builder.HasIndex(x => new { x.UserId, x.OccurredAt });
        builder.HasIndex(x => new { x.ProductId, x.OccurredAt });
        builder.HasIndex(x => new { x.UserId, x.ProductId, x.BehaviorType });

        // Relationships
        builder.HasOne(x => x.User)
            .WithMany(u => u.UserBehaviors)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Product)
            .WithMany(p => p.UserBehaviors)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}


