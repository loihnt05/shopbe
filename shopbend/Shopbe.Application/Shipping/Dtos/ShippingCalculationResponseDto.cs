namespace Shopbe.Application.Shipping.Dtos;

public sealed record ShippingCalculationResponseDto(
    decimal ShippingFee,
    string? ZoneName,
    DateTime EstimatedDeliveryDate,
    string Currency = "VND");
