using MediatR;
using Shopbe.Application.Interfaces;
using Shopbe.Application.Products.Dtos;

namespace Shopbe.Application.Products.Queries.GetAllProducts;

public class GetAllProductsHandler : IRequestHandler<GetAllProductsQuery, IEnumerable<ProductResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllProductsHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<ProductResponseDto>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await _unitOfWork.Product.GetAllProductsAsync();

        var filtered = products
            .Where(p =>
                (request.Filter.Name == null || p.Name.Contains(request.Filter.Name, StringComparison.OrdinalIgnoreCase)) &&
                (request.Filter.CategoryId == null || p.CategoryId == request.Filter.CategoryId) &&
                (request.Filter.MinPrice == null || p.BasePrice >= request.Filter.MinPrice) &&
                (request.Filter.MaxPrice == null || p.BasePrice <= request.Filter.MaxPrice))
            .Skip((request.Filter.PageNumber - 1) * request.Filter.PageSize)
            .Take(request.Filter.PageSize);

        return filtered.Select(ProductDtoMapper.ToResponse);
    }
}
