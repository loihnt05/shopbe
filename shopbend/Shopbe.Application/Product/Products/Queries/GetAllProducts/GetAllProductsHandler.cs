using MediatR;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.Product.Products.Dtos;

namespace Shopbe.Application.Product.Products.Queries.GetAllProducts;

public class GetAllProductsHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAllProductsQuery, IEnumerable<ProductResponseDto>>
{
    public async Task<IEnumerable<ProductResponseDto>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        // Query DTO is currently non-nullable on the request type, but keep this in case binding changes.
        var filter = request.Filter;

        var pageNumber = filter.PageNumber < 1 ? 1 : filter.PageNumber;
        var pageSize = filter.PageSize < 1 ? 20 : filter.PageSize;

        var page = await unitOfWork.Product.GetProductsPageAsync(
            filter.Name,
            filter.CategoryId,
            filter.MinBasePrice,
            filter.MaxBasePrice,
            pageNumber,
            pageSize,
            cancellationToken);

        return page.Select(ProductDtoMapper.ToResponse);
    }
}
