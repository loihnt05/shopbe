using MediatR;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.Product.ProductImages.Dtos;

namespace Shopbe.Application.Product.ProductImages.Queries.GetProductImageById;

public class GetProductImageByIdHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetProductImageByIdQuery, ProductImageResponseDto>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<ProductImageResponseDto> Handle(GetProductImageByIdQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}