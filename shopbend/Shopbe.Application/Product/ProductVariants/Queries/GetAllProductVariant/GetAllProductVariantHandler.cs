using MediatR;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.Product.ProductVariants.Dtos;

namespace Shopbe.Application.Product.ProductVariants.Queries.GetAllProductVariant;

public class GetAllProductVariantHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAllProductVariantQuery, IEnumerable<ProductVariantResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<IEnumerable<ProductVariantResponseDto>> Handle(GetAllProductVariantQuery request, CancellationToken cancellationToken)
    {
        var variants = await _unitOfWork.ProductVariant.GetAllProductVariantsAsync();
        var query = variants.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Filter.SKU))
        {
            query = query.Where(v => v.Sku.Contains(request.Filter.SKU.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        if (request.Filter.MinPrice is not null)
        {
            query = query.Where(v => v.Price >= request.Filter.MinPrice.Value);
        }

        if (request.Filter.MaxPrice is not null)
        {
            query = query.Where(v => v.Price <= request.Filter.MaxPrice.Value);
        }

        query = query
            .Skip((request.Filter.PageNumber - 1) * request.Filter.PageSize)
            .Take(request.Filter.PageSize);

        return query.Select(v => new ProductVariantResponseDto(
                v.Id,
                v.ProductId,
                v.Sku,
                v.Price,
                v.StockQuantity,
                v.IsActive,
                Array.Empty<Guid>())
            )
            .ToList();
    }
}