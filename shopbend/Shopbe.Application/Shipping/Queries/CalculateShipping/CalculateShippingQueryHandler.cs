using MediatR;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.Shipping.Dtos;

namespace Shopbe.Application.Shipping.Queries.CalculateShipping;

public sealed class CalculateShippingQueryHandler(IShippingCalculationService calculationService) 
    : IRequestHandler<CalculateShippingQuery, ShippingCalculationResponseDto>
{
    public async Task<ShippingCalculationResponseDto> Handle(CalculateShippingQuery request, CancellationToken cancellationToken)
    {
        var calcRequest = new ShippingCalculationRequestDto(
            request.City,
            request.District,
            request.Ward,
            request.Subtotal,
            request.TotalWeight,
            request.DistanceKm);

        return await calculationService.CalculateAsync(calcRequest, cancellationToken);
    }
}
