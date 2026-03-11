using System.Net;

namespace Shopbe.Domain.Entities;

public class Payment : BaseEntity
{
    public Guid OrderId {get; set;}
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string TransactionId { get; set; } = string.Empty;
}