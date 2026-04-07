using MediatR;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.Product.ProductVariants.Dtos;

namespace Shopbe.Application.Product.ProductVariants.Queries.GetProductVariantById;

public class GetProductVariantByIdHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetProductVariantByIdQuery, ProductVariantResponseDto>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<ProductVariantResponseDto> Handle(GetProductVariantByIdQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}