using MediatR;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.Product.ProductVariants.Dtos;

namespace Shopbe.Application.Product.ProductVariants.Queries.GetAllProductVariant;

public class GetAllProductVariantHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAllProductVariantQuery, IEnumerable<ProductVariantResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<IEnumerable<ProductVariantResponseDto>> Handle(GetAllProductVariantQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}