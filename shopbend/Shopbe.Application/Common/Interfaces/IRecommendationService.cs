using Shopbe.Application.Product.Products.Dtos;

namespace Shopbe.Application.Common.Interfaces;

public interface IRecommendationService
{
    // Trang chủ — top bán chạy
    Task<List<ProductResponseDto>> GetTopSellingAsync(int count = 10);

    // Trang sản phẩm — cùng danh mục
    Task<List<ProductResponseDto>> GetSimilarProductsAsync(int productId, int count = 8);

    // Dành cho user đã đăng nhập — dựa trên lịch sử
    Task<List<ProductResponseDto>> GetPersonalizedAsync(string userId, int count = 10);
}

