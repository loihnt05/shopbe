using MediatR;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.Product.Products.Dtos;

namespace Shopbe.Application.Product.Products.Queries.GetProductById;

public class GetProductByIdHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetProductByIdQuery, ProductResponseDto?>
{
    public async Task<ProductResponseDto?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await unitOfWork.Product.GetProductByIdAsync(request.Id);
        if (product is null)
            return null;

        return ProductDtoMapper.ToResponse(product);
    }
}
