using MediatR;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.Product.Products.Dtos;

namespace Shopbe.Application.Product.Products.Queries.GetAllProducts;

public class GetAllProductsHandler(IUnitOfWork unitOfWork, ICacheService cache)
    : IRequestHandler<GetAllProductsQuery, ProductSearchResponseDto>
{
    public async Task<ProductSearchResponseDto> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter;

        var pageNumber = filter.PageNumber < 1 ? 1 : filter.PageNumber;
        var pageSize = filter.PageSize < 1 ? 20 : filter.PageSize;

        var categoryIdsStr = filter.CategoryIds != null ? string.Join(',', filter.CategoryIds) : string.Empty;

        var cacheKey = string.Join('|', new[]
        {
            "products:search",
            $"name={filter.Name ?? string.Empty}",
            $"categoryIds={categoryIdsStr}",
            $"min={(filter.MinBasePrice?.ToString() ?? string.Empty)}",
            $"max={(filter.MaxBasePrice?.ToString() ?? string.Empty)}",
            $"page={pageNumber}",
            $"size={pageSize}"
        });

        var cached = await cache.GetAsync<ProductSearchResponseDto>(cacheKey);
        if (cached is not null)
        {
            return cached;
        }

        var products = await unitOfWork.Product.GetProductsPageAsync(
            filter.Name,
            filter.CategoryIds,
            filter.MinBasePrice,
            filter.MaxBasePrice,
            pageNumber,
            pageSize,
            cancellationToken);

        var totalCount = await unitOfWork.Product.GetTotalCountAsync(
            filter.Name,
            filter.CategoryIds,
            filter.MinBasePrice,
            filter.MaxBasePrice,
            cancellationToken);

        var categoryCounts = await unitOfWork.Product.GetCategoryCountsAsync(
            filter.Name,
            filter.CategoryIds,
            filter.MinBasePrice,
            filter.MaxBasePrice,
            cancellationToken);

        var categories = await unitOfWork.Category.GetAllCategoriesAsync();
        var facets = categories
            .Where(c => categoryCounts.ContainsKey(c.Id))
            .Select(c => new CategoryFacetDto(c.Id, c.Name, c.Slug, categoryCounts[c.Id]))
            .OrderByDescending(f => f.Count)
            .ToList();

        var productDtos = products.Select(ProductDtoMapper.ToResponse).ToList();
        
        var result = new ProductSearchResponseDto(productDtos, facets, totalCount);

        await cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(2));
        return result;
    }
}
