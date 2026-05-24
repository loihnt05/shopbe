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
        var categorySlugsStr = filter.CategorySlugs != null ? string.Join(',', filter.CategorySlugs) : string.Empty;
        var brandIdsStr = filter.BrandIds != null ? string.Join(',', filter.BrandIds) : string.Empty;

        var cacheKey = string.Join('|', new[]
        {
            "products:search:v4",
            $"name={filter.Name ?? string.Empty}",
            $"categoryIds={categoryIdsStr}",
            $"categorySlugs={categorySlugsStr}",
            $"brandIds={brandIdsStr}",
            $"min={(filter.minBasePrice?.ToString() ?? string.Empty)}",
            $"max={(filter.maxBasePrice?.ToString() ?? string.Empty)}",
            $"minRating={(filter.MinRating?.ToString() ?? string.Empty)}",
            $"sortBy={filter.SortBy ?? string.Empty}",
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
            filter.CategorySlugs,
            filter.BrandIds,
            filter.minBasePrice,
            filter.maxBasePrice,
            filter.MinRating,
            filter.SortBy,
            pageNumber,
            pageSize,
            cancellationToken);

        var totalCount = await unitOfWork.Product.GetTotalCountAsync(
            filter.Name,
            filter.CategoryIds,
            filter.CategorySlugs,
            filter.BrandIds,
            filter.minBasePrice,
            filter.maxBasePrice,
            filter.MinRating,
            cancellationToken);

        var categoryCounts = await unitOfWork.Product.GetCategoryCountsAsync(
            filter.Name,
            filter.CategoryIds,
            filter.CategorySlugs,
            filter.BrandIds,
            filter.minBasePrice,
            filter.maxBasePrice,
            filter.MinRating,
            cancellationToken);

        var brandCounts = await unitOfWork.Product.GetBrandCountsAsync(
            filter.Name,
            filter.CategoryIds,
            filter.CategorySlugs,
            filter.BrandIds,
            filter.minBasePrice,
            filter.maxBasePrice,
            filter.MinRating,
            cancellationToken);

        var categories = await unitOfWork.Category.GetAllCategoriesAsync();
        var categoryFacets = categories
            .Where(c => categoryCounts.ContainsKey(c.Id))
            .Select(c => new CategoryFacetDto(c.Id, c.Name, c.Slug, categoryCounts[c.Id]))
            .OrderByDescending(f => f.Count)
            .ToList();

        var brands = await unitOfWork.Brand.GetAllBrandsAsync();
        var brandFacets = brands
            .Where(b => brandCounts.ContainsKey(b.Id))
            .Select(b => new BrandFacetDto(b.Id, b.Name, b.Slug, brandCounts[b.Id]))
            .OrderByDescending(f => f.Count)
            .ToList();

        var productDtos = products.Select(p => ProductDtoMapper.ToResponse(p, null)).ToList();
        
        var result = new ProductSearchResponseDto(productDtos, categoryFacets, brandFacets, totalCount);

        await cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(2));
        return result;
    }
}
