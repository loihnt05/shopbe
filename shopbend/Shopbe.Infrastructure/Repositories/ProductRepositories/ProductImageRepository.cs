using Microsoft.EntityFrameworkCore;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Domain.Entities;
using Shopbe.Domain.Entities.Product;
using Shopbe.Infrastructure.Persistence;

namespace Shopbe.Infrastructure.Repositories;

public class ProductImageRepository(ShopDbContext context) : IProductImageRepository
{
    public async Task<ProductImage?> GetProductImageByIdAsync(Guid productImageId)
    {
        return await context.ProductImages.FindAsync(productImageId);
    }
    public async Task<IEnumerable<ProductImage>> GetProductImagesByProductIdAsync(Guid productId)
    {
        return await context.ProductImages
            .Where(pi => pi.ProductId == productId)
            .ToListAsync();
    }
    public async Task<IEnumerable<ProductImage>> GetAllProductImagesAsync()
    {
        return await context.ProductImages.ToListAsync();
    }
    public async Task DeleteProductImageAsync(Guid productImageId)
    {
        var productImage = await context.ProductImages.FindAsync(productImageId);
        if (productImage != null)
        {
            context.ProductImages.Remove(productImage);
        }
    }
    public async Task AddProductImageAsync(ProductImage productImage)
    {
        await context.ProductImages.AddAsync(productImage);
    }
    public async Task UpdateProductImageAsync(ProductImage productImage)
    {
        context.ProductImages.Update(productImage);
    }
}