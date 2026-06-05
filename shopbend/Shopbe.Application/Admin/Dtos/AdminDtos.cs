using Shopbe.Application.Order.Dtos;
using Shopbe.Domain.Enums;

namespace Shopbe.Application.Admin.Dtos;

public sealed record AdminDashboardOverviewDto(
    int TotalUsers,
    int TotalCustomers,
    int TotalSellers,
    int TotalProducts,
    int PendingProducts,
    int TotalOrders,
    int PendingOrders,
    int CompletedOrders,
    decimal TotalRevenue,
    decimal MonthlyRevenue,
    IReadOnlyList<AdminTopProductDto> TopProducts,
    IReadOnlyList<AdminTopSellerDto> TopSellers,
    IReadOnlyList<AdminOrderListItemDto> RecentOrders,
    IReadOnlyList<AdminUserDto> RecentUsers
);

public sealed record AdminUserQueryDto(
    string? Search = null,
    UserRole? Role = null,
    UserStatus? Status = null,
    int Page = 1,
    int PageSize = 20
);

public sealed record AdminUserDto(
    Guid Id,
    string KeycloakId,
    string Email,
    string FullName,
    UserRole? Role,
    UserStatus? Status,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? DeletedAt
);

public sealed record AdminSellerQueryDto(
    string? Search = null,
    SellerStatus? Status = null,
    int Page = 1,
    int PageSize = 20
);

public sealed record AdminSellerDto(
    Guid UserId,
    Guid? SellerProfileId,
    string OwnerName,
    string OwnerEmail,
    string ShopName,
    SellerStatus Status,
    int TotalSales,
    decimal TotalRevenue,
    decimal? Rating,
    decimal CommissionRate,
    DateTime CreatedAt
);

public sealed record AdminSellerStatsDto(
    Guid UserId,
    string ShopName,
    int TotalProducts,
    int ApprovedProducts,
    int PendingProducts,
    int TotalOrders,
    int CompletedOrders,
    decimal TotalRevenue
);

public sealed record AdminProductQueryDto(
    string? Search = null,
    ApprovalStatus? Status = null,
    int Page = 1,
    int PageSize = 20
);

public sealed record AdminProductDto(
    Guid Id,
    string Name,
    string Slug,
    string SellerName,
    string? ShopName,
    string? CategoryName,
    decimal Price,
    ApprovalStatus ApprovalStatus,
    bool IsActive,
    string? AdminNotes,
    DateTime CreatedAt
);

public sealed record AdminOrderQueryDto(
    string? Search = null,
    OrderStatus? Status = null,
    int Page = 1,
    int PageSize = 20
);

public sealed record AdminOrderListItemDto(
    Guid Id,
    Guid UserId,
    string CustomerName,
    string CustomerEmail,
    decimal TotalAmount,
    string Currency,
    OrderStatus Status,
    int ItemsCount,
    DateTime CreatedAt
);

public sealed record AdminTopProductDto(
    Guid ProductId,
    string ProductName,
    int SoldCount,
    decimal Revenue
);

public sealed record AdminTopSellerDto(
    Guid SellerId,
    string SellerName,
    string? ShopName,
    int TotalSales,
    decimal Revenue
);

public sealed record AdminRevenuePointDto(
    string Label,
    decimal Revenue,
    int Orders
);

public sealed record AdminSalesBreakdownDto(
    IReadOnlyList<AdminRevenuePointDto> RevenueByPeriod,
    IReadOnlyList<KeyValuePair<string, decimal>> SalesByStatus
);

public sealed record AdminUpdateUserStatusRequestDto(UserStatus Status);

public sealed record AdminUpdateUserRoleRequestDto(UserRole Role);

public sealed record AdminUpdateSellerStatusRequestDto(SellerStatus Status);

public sealed record AdminUpdateProductApprovalRequestDto(ApprovalStatus Status, string? AdminNotes);

public sealed record AdminUpdateProductVisibilityRequestDto(bool IsActive);
