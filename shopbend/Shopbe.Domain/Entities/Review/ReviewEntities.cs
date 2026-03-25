namespace Shopbe.Domain.Entities;

public class Review : BaseEntity
{
    public Guid ProductId { get; set; }
    public Guid UserId { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;

    // Navigation properties
    public Product? Product { get; set; }
    public User? User { get; set; }
    public ICollection<ReviewImage> ReviewImages { get; set; } = new List<ReviewImage>();
}

public class ReviewImage : BaseEntity
{
    public Guid ReviewId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;

    // Navigation properties
    public Review? Review { get; set; }
}

