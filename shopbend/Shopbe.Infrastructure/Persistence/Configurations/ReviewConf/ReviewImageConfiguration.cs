using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shopbe.Domain.Entities.Review;

namespace Shopbe.Infrastructure.Persistence.Configurations.ReviewConf;

public class ReviewImageConfiguration : IEntityTypeConfiguration<ReviewImage>
{
    public void Configure(EntityTypeBuilder<ReviewImage> builder)
    {
        builder.ToTable("ReviewImages");

        builder.HasKey(ri => ri.Id);

        builder.Property(ri => ri.ReviewId)
            .IsRequired();

        builder.Property(ri => ri.ImageUrl)
            .IsRequired()
            .HasMaxLength(2048);

        builder.Property(ri => ri.SortOrder)
            .IsRequired();

        builder.HasIndex(ri => ri.ReviewId);
        builder.HasIndex(ri => new { ri.ReviewId, ri.SortOrder });

        builder.HasOne(ri => ri.Review)
            .WithMany(r => r.ReviewImages)
            .HasForeignKey(ri => ri.ReviewId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}