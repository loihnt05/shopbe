using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shopbe.Domain.Entities.ShoppingCart;
using Shopbe.Domain.Entities.User;

namespace Shopbe.Infrastructure.Persistence.Configurations.UserConf;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.KeycloakId)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(u => u.KeycloakId)
            .IsUnique();

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.Property(u => u.FullName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(u => u.AvatarUrl)
            .HasMaxLength(2048);

        builder.Property(u => u.PhoneNumber)
            .HasMaxLength(20);

        builder.Property(u => u.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasDefaultValue(UserStatus.PendingVerification);

        builder.HasIndex(u => u.Status);

        builder.Property(u => u.DeletedAt)
            .IsRequired(false);

        builder.HasIndex(u => u.DeletedAt);

        builder.Property(u => u.Role)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(u => u.Role)
            .HasDefaultValue(UserRole.Customer);

        // Relationships
        builder.HasMany(u => u.UserAddresses)
            .WithOne(ua => ua.User)
            .HasForeignKey(ua => ua.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.Orders)
            .WithOne(o => o.User)
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(u => u.ShoppingCart)
            .WithOne(sc => sc.User)
            .HasForeignKey<ShoppingCart>(sc => sc.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
