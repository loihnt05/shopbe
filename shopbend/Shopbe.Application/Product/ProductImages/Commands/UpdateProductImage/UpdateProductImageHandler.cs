using MediatR;
using Shopbe.Application.Interfaces;
using Shopbe.Application.ProductsImages.Dtos;

namespace Shopbe.Application.ProductsImages.Commands.UpdateProductImage;

public class UpdateProductImageHandler : IRequestHandler<UpdateProductImageCommand, ProductImageResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProductImageHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    
    public Task<ProductImageResponseDto> Handle(UpdateProductImageCommand request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new ProductImageResponseDto(request.Id, request.Request.ImageUrl, request.Request.IsPrimary));
    }
}