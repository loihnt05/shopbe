using MediatR;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.ProductsImages.Dtos;

namespace Shopbe.Application.ProductsImages.Commands.CreateProductImage;

public class CreateProductImageHandler : IRequestHandler<CreateProductImageCommand, ProductImageResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateProductImageHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    public async Task<ProductImageResponseDto> Handle(CreateProductImageCommand command, CancellationToken cancellationToken)
    {
        return new ProductImageResponseDto(Guid.NewGuid(), command.Request.ImageUrl, command.Request.IsPrimary);
    }
}