namespace Shopbe.Application.Order.Dtos;

public sealed class SelectedCartItemDto
{
    public Guid ProductVariantId { get; set; }

    /// <summary>
    /// Quantity to checkout for this variant. Must be > 0 and &lt;= quantity currently in cart.
    /// </summary>
    public int Quantity { get; set; }
}


