using MediatR;
using Shopbe.Application.Shipping.Dtos;

namespace Shopbe.Application.Shipping.Queries.CalculateShipping;

public sealed record CalculateShippingQuery(
    string City,
    string District,
    string? Ward,
    decimal Subtotal,
    double TotalWeight = 0,
    double DistanceKm = 0) : IRequest<ShippingCalculationResponseDto>;
