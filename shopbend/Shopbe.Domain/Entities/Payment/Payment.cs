namespace Shopbe.Domain.Entities;

public class Payment : BaseEntity
{
    public Guid OrderId { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string TransactionId { get; set; } = string.Empty;
    
    // Navigation Properties
    public Order? Order { get; set; }
    public ICollection<PaymentTransaction> PaymentTransactions { get; set; } = new List<PaymentTransaction>();
    public ICollection<PaymentLog> PaymentLogs { get; set; } = new List<PaymentLog>();
    public ICollection<Refund> Refunds { get; set; } = new List<Refund>();
}