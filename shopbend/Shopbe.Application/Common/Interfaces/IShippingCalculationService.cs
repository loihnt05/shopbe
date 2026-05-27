using Shopbe.Application.Shipping.Dtos;

namespace Shopbe.Application.Common.Interfaces;

public interface IShippingCalculationService
{
    Task<ShippingCalculationResponseDto> CalculateAsync(ShippingCalculationRequestDto request, CancellationToken ct = default);
}
