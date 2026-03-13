using MediatR;
using Shopbe.Application.Interfaces;
using Shopbe.Application.Products.Dtos;

namespace Shopbe.Application.Products.Commands.UpdateProduct;

public class UpdateProductHandler : IRequestHandler<UpdateProductCommand, ProductResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProductHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ProductResponseDto> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Product.GetProductByIdAsync(request.Id);
        if (product is null)
            throw new KeyNotFoundException($"Product with id '{request.Id}' was not found.");

        if (string.IsNullOrWhiteSpace(request.Request.Name))
            throw new ArgumentException("Product name is required.");

        if (request.Request.Price < 0)
            throw new ArgumentException("Product price cannot be negative.");

        if (request.Request.StockQuantity < 0)
            throw new ArgumentException("Stock quantity cannot be negative.");

        var category = await _unitOfWork.Category.GetCategoryByIdAsync(request.Request.CategoryId);
        if (category is null)
            throw new KeyNotFoundException($"Category with id '{request.Request.CategoryId}' was not found.");

        product.Name = request.Request.Name;
        product.Description = request.Request.Description;
        product.Price = request.Request.Price;
        product.ImageUrl = request.Request.ImageUrl;
        product.StockQuantity = request.Request.StockQuantity;
        product.CategoryId = request.Request.CategoryId;

        await _unitOfWork.Product.UpdateProductAsync(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ProductResponseDto(
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            product.ImageUrl,
            product.StockQuantity,
            product.CategoryId
        );
    }
}
