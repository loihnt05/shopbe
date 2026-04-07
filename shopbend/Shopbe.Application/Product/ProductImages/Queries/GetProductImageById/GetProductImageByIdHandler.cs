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
        var image = await _unitOfWork.ProductImage.GetProductImageByIdAsync(request.Id);
        if (image is null)
        {
            throw new KeyNotFoundException($"Product image with id '{request.Id}' was not found.");
        }

        return new ProductImageResponseDto(image.Id, image.ImageUrl, image.IsPrimary);
    }
}