namespace Shopbe.Application.Product.Products.Dtos;

public class DummyProductDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal DiscountPercentage { get; set; }
    public double Rating { get; set; }
    public int Stock { get; set; }
    public string? Brand { get; set; }
    public string? Sku { get; set; }
    public List<string>? Images { get; set; }
    public string? Thumbnail { get; set; }
}

public class DummyResponseDto
{
    public List<DummyProductDto> Products { get; set; } = new();
    public int Total { get; set; }
    public int Skip { get; set; }
    public int Limit { get; set; }
}
