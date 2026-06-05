using Shopbe.Application.Order.Dtos;
using Shopbe.Application.Product.ProductImages.Dtos;
using Shopbe.Application.Product.ProductVariants.Dtos;
using Shopbe.Domain.Enums;

namespace Shopbe.Application.Seller.Dtos;

public sealed record SellerDashboardOverviewDto(
    int MyProducts,
    int PendingOrders,
    int ProcessingOrders,
    int ShippedOrders,
    int DeliveredOrders,
    decimal TotalRevenue,
    decimal ThisMonthRevenue,
    IReadOnlyList<SellerLowStockProductDto> LowStockProducts,
    IReadOnlyList<SellerOrderListItemDto> RecentOrders,
    IReadOnlyList<SellerProductListItemDto> BestSellingProducts
);

public sealed record SellerProductQueryDto(
    string? Search = null,
    ApprovalStatus? Status = null,
    int Page = 1,
    int PageSize = 20
);

public sealed record SellerProductUpsertDto(
    string Name,
    string Description,
    decimal BasePrice,
    Guid CategoryId,
    Guid? BrandId,
    bool IsActive,
    IEnumerable<ProductImageRequestDto>? Images,
    IEnumerable<ProductVariantRequestDto>? Variants
);

public sealed record SellerProductListItemDto(
    Guid Id,
    string Name,
    string Slug,
    string? CategoryName,
    decimal Price,
    int Stock,
    ApprovalStatus ApprovalStatus,
    bool IsActive,
    DateTime CreatedAt
);

public sealed record SellerOrderQueryDto(
    string? Search = null,
    OrderStatus? Status = null,
    int Page = 1,
    int PageSize = 20
);

public sealed record SellerOrderListItemDto(
    Guid Id,
    string CustomerName,
    string CustomerEmail,
    int SellerItemsCount,
    decimal SellerItemsTotal,
    OrderStatus Status,
    DateTime CreatedAt
);

public sealed record SellerUpdateOrderStatusRequestDto(OrderStatus Status, string? Note = null);

public sealed record SellerRevenuePointDto(string Label, decimal Revenue, int Orders);

public sealed record SellerLowStockProductDto(Guid ProductId, string ProductName, int Stock);

public sealed record SellerProfileDto(
    Guid Id,
    Guid UserId,
    string ShopName,
    string? ShopDescription,
    string? ShopLogoUrl,
    string? ShopBannerUrl,
    string? ContactPhone,
    string? ContactEmail,
    string? Address,
    string? City,
    SellerStatus Status,
    decimal CommissionRate,
    decimal? Rating,
    int TotalSales,
    decimal TotalRevenue
);

public sealed record SellerProfileUpsertDto(
    string ShopName,
    string? ShopDescription,
    string? ShopLogoUrl,
    string? ShopBannerUrl,
    string? ContactPhone,
    string? ContactEmail,
    string? Address,
    string? City
);
