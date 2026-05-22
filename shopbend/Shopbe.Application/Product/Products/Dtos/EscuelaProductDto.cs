namespace Shopbe.Application.Product.Products.Dtos;

public class EscuelaProductDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
    public EscuelaCategoryDto Category { get; set; } = new();
    public List<string> Images { get; set; } = new();
}

public class EscuelaCategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
}
