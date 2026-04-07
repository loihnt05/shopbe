using MediatR;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.Product.ProductImages.Dtos;

namespace Shopbe.Application.Product.ProductImages.Queries.GetAllProductImage;

public class GetAllProductImageHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAllProductImageQuery, IEnumerable<ProductImageResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<IEnumerable<ProductImageResponseDto>> Handle(GetAllProductImageQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}