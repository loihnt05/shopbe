using MediatR;
using Shopbe.Application.Interfaces;
using Shopbe.Application.Products.Dtos;

namespace Shopbe.Application.Products.Queries.GetProductById;

public class GetProductByIdHandler : IRequestHandler<GetProductByIdQuery, ProductResponseDto?>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetProductByIdHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ProductResponseDto?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Product.GetProductByIdAsync(request.Id);
        if (product is null)
            return null;

        return new ProductResponseDto(
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            product.ImageUrl,
            product.StockQuantity,
            product.CategoryId
        );
    }
}
