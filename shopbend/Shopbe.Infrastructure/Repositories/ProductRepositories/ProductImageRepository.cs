using Microsoft.EntityFrameworkCore;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Domain.Entities;
using Shopbe.Domain.Entities.Product;
using Shopbe.Infrastructure.Persistence;

namespace Shopbe.Infrastructure.Repositories;

public class ProductImageRepository : IProductImageRepository
{
    private readonly ShopDbContext _context;

    public ProductImageRepository(ShopDbContext context)
    {
        _context = context;
    }
    public async Task<ProductImage?> GetProductImageByIdAsync(Guid productImageId)
    {
        return await _context.ProductImages.FindAsync(productImageId);
    }
    public async Task<IEnumerable<ProductImage>> GetProductImagesByProductIdAsync(Guid productId)
    {
        return await _context.ProductImages
            .Where(pi => pi.ProductId == productId)
            .ToListAsync();
    }
    public async Task<IEnumerable<ProductImage>> GetAllProductImagesAsync()
    {
        return await _context.ProductImages.ToListAsync();
    }
    public async Task DeleteProductImageAsync(Guid productImageId)
    {
        var productImage = await _context.ProductImages.FindAsync(productImageId);
        if (productImage != null)
        {
            _context.ProductImages.Remove(productImage);
        }
    }
    public async Task AddProductImageAsync(ProductImage productImage)
    {
        await _context.ProductImages.AddAsync(productImage);
    }
    public async Task UpdateProductImageAsync(ProductImage productImage)
    {
        _context.ProductImages.Update(productImage);
    }
}