namespace Shopbe.Domain.Entities.Review;

public class Review : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid ProductId { get; set; }
    public Guid OrderId { get; set; }
    public short Rating { get; set; }
    public string? Comment { get; set; }
    public bool IsVisible { get; set; }

    // Navigation properties
    public User.User? User { get; set; }
    public Product.Product? Product { get; set; }
    public Order.Order? Order { get; set; }
    public ICollection<ReviewImage> ReviewImages { get; set; } = new List<ReviewImage>();
}
