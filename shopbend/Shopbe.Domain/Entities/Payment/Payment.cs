namespace Shopbe.Domain.Entities.Payment;

public class Payment : BaseEntity
{
    public Guid OrderId { get; set; }
    public PaymentMethod Method { get; set; }
    public PaymentStatus Status { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "VND";
    public DateTime? PaidAt { get; set; }
    
    // Navigation Properties
    public Order.Order? Order { get; set; }
    public ICollection<PaymentTransaction> PaymentTransactions { get; set; } = new List<PaymentTransaction>();
    public ICollection<PaymentLog> PaymentLogs { get; set; } = new List<PaymentLog>();
    public ICollection<Refund> Refunds { get; set; } = new List<Refund>();
}