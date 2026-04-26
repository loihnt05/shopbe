using MediatR;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.Product.Products.Dtos;

namespace Shopbe.Application.Product.Products.Queries.GetAllProducts;

public class GetAllProductsHandler(IUnitOfWork unitOfWork, ICacheService cache)
    : IRequestHandler<GetAllProductsQuery, IEnumerable<ProductResponseDto>>
{
    public async Task<IEnumerable<ProductResponseDto>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        // Query DTO is currently non-nullable on the request type, but keep this in case binding changes.
        var filter = request.Filter;

        var pageNumber = filter.PageNumber < 1 ? 1 : filter.PageNumber;
        var pageSize = filter.PageSize < 1 ? 20 : filter.PageSize;

        // Cache key should uniquely represent the filter inputs.
        // Note: keep it stable and simple (avoid culture-sensitive formatting).
        var cacheKey = string.Join('|', new[]
        {
            "products:page",
            $"name={filter.Name ?? string.Empty}",
            $"categoryId={(filter.CategoryId?.ToString() ?? string.Empty)}",
            $"min={(filter.MinBasePrice?.ToString() ?? string.Empty)}",
            $"max={(filter.MaxBasePrice?.ToString() ?? string.Empty)}",
            $"page={pageNumber}",
            $"size={pageSize}"
        });

        var cached = await cache.GetAsync<List<ProductResponseDto>>(cacheKey);
        if (cached is not null)
        {
            return cached;
        }

        var page = await unitOfWork.Product.GetProductsPageAsync(
            filter.Name,
            filter.CategoryId,
            filter.MinBasePrice,
            filter.MaxBasePrice,
            pageNumber,
            pageSize,
            cancellationToken);

        var dtos = page.Select(ProductDtoMapper.ToResponse).ToList();
        await cache.SetAsync(cacheKey, dtos, TimeSpan.FromMinutes(2));
        return dtos;
    }
}
