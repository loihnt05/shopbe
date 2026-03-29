using MediatR;
using Shopbe.Application.Interfaces;
using Shopbe.Application.ProductsImages.Dtos;

namespace Shopbe.Application.ProductsImages.Queries.GetAllProductImage;

public class GetAllProductImageHandler : IRequestHandler<GetAllProductImageQuery, IEnumerable<ProductImageResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllProductImageHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<ProductImageResponseDto>> Handle(GetAllProductImageQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}