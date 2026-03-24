using MediatR;
using Shopbe.Application.Interfaces;
using Shopbe.Application.ProductsVariants.Queries.GetProductVariantById;
using Shopbe.Application.ProductVariants.Dtos;

namespace Shopbe.Application.ProductsVariants.Queires.GetProductVariantById;

public class GetProductVariantByIdHandler : IRequestHandler<GetProductVariantByIdQuery, ProductVariantResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetProductVariantByIdHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ProductVariantResponseDto> Handle(GetProductVariantByIdQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}