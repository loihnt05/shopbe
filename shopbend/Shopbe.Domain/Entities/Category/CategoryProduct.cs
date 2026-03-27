namespace Shopbe.Domain.Entities.Category;

public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public Guid? ParentCategoryId { get; set; }
    
    // Navigation Properties
    public Category? ParentCategory { get; set; }
    public ICollection<Category> ChildCategories { get; set; } = new List<Category>();
    public ICollection<Product.Product> Products { get; set; } = new List<Product.Product>();
}