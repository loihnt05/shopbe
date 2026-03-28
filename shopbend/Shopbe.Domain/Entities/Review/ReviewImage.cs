namespace Shopbe.Domain.Entities.Review;

public class ReviewImage : BaseEntity
{
    public Guid ReviewId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public int SortOrder { get; set; }

    // Navigation Properties
    public Review? Review { get; set; }
}

