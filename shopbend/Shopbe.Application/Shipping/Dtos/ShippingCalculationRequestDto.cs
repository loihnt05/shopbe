namespace Shopbe.Application.Shipping.Dtos;

public sealed record ShippingCalculationRequestDto(
    string City,
    string District,
    string? Ward,
    decimal Subtotal,
    double TotalWeight = 0,
    double DistanceKm = 0);
