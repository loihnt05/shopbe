namespace Shopbe.Domain.Entities.Shipping;

public enum ShipmentStatus
{
    Pending,
    PickedUp,
    InTransit,
    OutForDelivery,
    Delivered,
    Failed,
    Returned
}

