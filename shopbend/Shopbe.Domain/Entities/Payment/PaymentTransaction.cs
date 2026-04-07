using System.Text.Json;
using Shopbe.Domain.Enums;

namespace Shopbe.Domain.Entities.Payment;

public class PaymentTransaction : BaseEntity
{
    public Guid PaymentId { get; set; }
    public string? GatewayTransactionId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "VND";
    public PaymentStatus Status { get; set; }
    public JsonDocument? GatewayResponse { get; set; }

    // Navigation Properties
    public Payment? Payment { get; set; }
}

