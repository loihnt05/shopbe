using MediatR;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.ProductsImages.Dtos;

namespace Shopbe.Application.ProductsImages.Queries.GetProductImageById;

public class GetProductImageByIdHandler : IRequestHandler<GetProductImageByIdQuery, ProductImageResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetProductImageByIdHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ProductImageResponseDto> Handle(GetProductImageByIdQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}