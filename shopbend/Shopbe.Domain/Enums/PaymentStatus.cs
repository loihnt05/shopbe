namespace Shopbe.Domain.Entities.Payment;

public enum PaymentStatus
{
    Pending,
    Authorized,
    Paid,
    Failed,
    Refunded,
    PartiallyRefunded
}

