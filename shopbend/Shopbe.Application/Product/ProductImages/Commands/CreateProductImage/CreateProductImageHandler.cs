using MediatR;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.Product.ProductImages.Dtos;

namespace Shopbe.Application.Product.ProductImages.Commands.CreateProductImage;

public class CreateProductImageHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<CreateProductImageCommand, ProductImageResponseDto>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<ProductImageResponseDto> Handle(CreateProductImageCommand command, CancellationToken cancellationToken)
    {
        return new ProductImageResponseDto(Guid.NewGuid(), command.Request.ImageUrl, command.Request.IsPrimary);
    }
}