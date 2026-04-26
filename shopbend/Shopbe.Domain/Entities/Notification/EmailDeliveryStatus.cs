namespace Shopbe.Domain.Entities.Notification;

public enum EmailDeliveryStatus
{
    Pending = 0,
    Sent = 1,
    Failed = 2,
    DeadLetter = 3
}

