namespace Shopbe.Domain.Entities.Payment;

public class PaymentLog : BaseEntity
{
    public Guid PaymentId { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Level { get; set; } = "info";

    // Navigation Properties
    public Payment? Payment { get; set; }
}

