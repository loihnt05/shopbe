using Shopbe.Domain.Entities.User;

namespace Shopbe.Domain.Entities.Order;

public class OrderStatusHistory : BaseEntity
{
    public Guid OrderId { get; set; }
    public OrderStatus Status { get; set; }
    public string? Note { get; set; }
    public Guid? ChangedBy { get; set; }
    public DateTime ChangedAt { get; set; }

    // Navigation Properties
    public Order? Order { get; set; }
    public User.User? User { get; set; }
}

