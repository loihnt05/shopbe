using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shopbe.Domain.Entities;

namespace Shopbe.Infrastructure.Persistence.Configurations;

public class UserAddressConfiguration : IEntityTypeConfiguration<UserAddress>
{
    public void Configure(EntityTypeBuilder<UserAddress> builder)
    {
        builder.ToTable("UserAddresses");

        builder.HasKey(ua => ua.Id);

        builder.Property(ua => ua.UserId)
            .IsRequired();

        builder.Property(ua => ua.Street)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(ua => ua.City)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(ua => ua.State)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(ua => ua.PostalCode)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(ua => ua.Country)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(ua => ua.IsDefault)
            .HasDefaultValue(false);

        // Relationships
        builder.HasOne(ua => ua.User)
            .WithMany(u => u.UserAddresses)
            .HasForeignKey(ua => ua.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
