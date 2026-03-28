using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shopbe.Domain.Entities.User;

namespace Shopbe.Infrastructure.Persistence.Configurations.UserConf;

public class UserAddressConfiguration : IEntityTypeConfiguration<UserAddress>
{
    public void Configure(EntityTypeBuilder<UserAddress> builder)
    {
        builder.ToTable("UserAddresses");

        builder.HasKey(ua => ua.Id);

        builder.Property(ua => ua.UserId)
            .IsRequired();

        builder.HasIndex(ua => ua.UserId);

        builder.Property(ua => ua.ReceiverName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(ua => ua.Phone)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(ua => ua.AddressLine)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(ua => ua.City)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(ua => ua.District)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(ua => ua.Ward)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(ua => ua.DeletedAt)
            .IsRequired(false);

        builder.HasIndex(ua => ua.DeletedAt);

        builder.Property(ua => ua.IsDefault)
            .HasDefaultValue(false);

        // Enforce a single default address per user (works on PostgreSQL)
        builder.HasIndex(ua => ua.UserId)
            .IsUnique()
            .HasFilter("\"IsDefault\" = true AND \"DeletedAt\" IS NULL");

    }
}
