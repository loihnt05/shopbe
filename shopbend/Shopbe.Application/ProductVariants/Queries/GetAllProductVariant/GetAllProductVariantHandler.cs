using MediatR;
using Shopbe.Application.Interfaces;
using Shopbe.Application.ProductsVariants.Queries.GetAllProducts;
using Shopbe.Application.ProductVariants.Dtos;

namespace Shopbe.Application.ProductsVariants.Queires.GetAllProductVariant;

public class GetAllProductVariantHandler : IRequestHandler<GetAllProductVariantQuery, IEnumerable<ProductVariantResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllProductVariantHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<ProductVariantResponseDto>> Handle(GetAllProductVariantQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}