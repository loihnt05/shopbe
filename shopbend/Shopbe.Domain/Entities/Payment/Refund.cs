using Shopbe.Domain.Entities.User;
using Shopbe.Domain.Enums;

namespace Shopbe.Domain.Entities.Payment;

public class Refund : BaseEntity
{
    public Guid PaymentId { get; set; }
    public decimal Amount { get; set; }
    public string? Reason { get; set; }
    public RefundStatus Status { get; set; }
    public Guid? ProcessedBy { get; set; }

    // Navigation Properties
    public Payment? Payment { get; set; }
    public User.User? User { get; set; }
}

