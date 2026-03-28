using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shopbe.Domain.Entities.Category;

namespace Shopbe.Infrastructure.Persistence.Configurations.CategoryConf;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");
        
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Slug)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(c => c.SortOrder)
            .IsRequired();

        builder.Property(c => c.IsActive)
            .IsRequired();

        // Self-referencing relationship for parent categories
        builder.HasOne(c => c.ParentCategory)
            .WithMany(c => c.SubCategories)
            .HasForeignKey(c => c.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasIndex(c => c.Slug)
            .IsUnique();

        builder.HasIndex(c => c.ParentCategoryId);
    }
}