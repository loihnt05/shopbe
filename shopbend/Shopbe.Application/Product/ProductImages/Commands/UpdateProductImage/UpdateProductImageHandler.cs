using MediatR;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.Product.ProductImages.Dtos;

namespace Shopbe.Application.Product.ProductImages.Commands.UpdateProductImage;

public class UpdateProductImageHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateProductImageCommand, ProductImageResponseDto>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public Task<ProductImageResponseDto> Handle(UpdateProductImageCommand request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new ProductImageResponseDto(request.Id, request.Request.ImageUrl, request.Request.IsPrimary));
    }
}