using MediatR;
using Shopbe.Application.Interfaces;
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
        // var product = await _unitOfWork.Product.GetProductByIdAsync(command.Id);
        // if (product is null)
        // {
        //     throw new KeyNotFoundException($"Product with id '{command.Id}' was not found.");
        // }

        // var productImage = new Domain.Entities.ProductImage
        // {
        //     Id = Guid.NewGuid(),
        //     ImageUrl = command.Request.ImageUrl,
        //     IsPrimary = command.Request.IsPrimary,
        //     ProductId = command.Id
        // };

        // await _unitOfWork.ProductImage.AddAsync(productImage);
        // await _unitOfWork.SaveChangesAsync();

        // return new ProductImageResponseDto(productImage.Id, productImage.ImageUrl, productImage.IsPrimary);
        return new ProductImageResponseDto(Guid.NewGuid(), command.Request.ImageUrl, command.Request.IsPrimary);
    }
}