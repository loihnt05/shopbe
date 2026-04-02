using MediatR;
using Shopbe.Application.Interfaces;
using Shopbe.Application.Product.Products.Dtos;

namespace Shopbe.Application.Product.Products.Queries.GetAllProducts;

public class GetAllProductsHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAllProductsQuery, IEnumerable<ProductResponseDto>>
{
    public async Task<IEnumerable<ProductResponseDto>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await unitOfWork.Product.GetAllProductsAsync();

        // Query DTO is currently non-nullable on the request type, but keep this in case binding changes.
        var filter = request.Filter;

        var pageNumber = filter.PageNumber < 1 ? 1 : filter.PageNumber;
        var pageSize = filter.PageSize < 1 ? 20 : filter.PageSize;

        var filtered = products
            .Where(p =>
                (string.IsNullOrWhiteSpace(filter.Name) || p.Name.Contains(filter.Name, StringComparison.OrdinalIgnoreCase)) &&
                (filter.CategoryId == null || p.CategoryId == filter.CategoryId) &&
                (filter.MinPrice == null || p.BasePrice >= filter.MinPrice) &&
                (filter.MaxPrice == null || p.BasePrice <= filter.MaxPrice))
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(ProductDtoMapper.ToResponse);

        return filtered;
    }
}
