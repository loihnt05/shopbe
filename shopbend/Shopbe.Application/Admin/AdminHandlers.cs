using MediatR;
using Shopbe.Application.Admin.Dtos;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.Order;
using Shopbe.Application.Order.Dtos;
using Shopbe.Domain.Entities.Seller;
using Shopbe.Domain.Enums;
using OrderEntity = Shopbe.Domain.Entities.Order.Order;
using UserEntity = Shopbe.Domain.Entities.User.User;

namespace Shopbe.Application.Admin;

public sealed record GetAdminDashboardOverviewQuery() : IRequest<AdminDashboardOverviewDto>;
public sealed record GetAdminUsersQuery(AdminUserQueryDto Query) : IRequest<PagedResultDto<AdminUserDto>>;
public sealed record GetAdminUserByIdQuery(Guid UserId) : IRequest<AdminUserDto?>;
public sealed record UpdateAdminUserStatusCommand(Guid UserId, AdminUpdateUserStatusRequestDto Request) : IRequest<AdminUserDto>;
public sealed record UpdateAdminUserRoleCommand(Guid UserId, AdminUpdateUserRoleRequestDto Request) : IRequest<AdminUserDto>;
public sealed record DeleteAdminUserCommand(Guid UserId) : IRequest<bool>;
public sealed record GetAdminSellersQuery(AdminSellerQueryDto Query) : IRequest<PagedResultDto<AdminSellerDto>>;
public sealed record GetAdminSellerByIdQuery(Guid SellerId) : IRequest<AdminSellerDto?>;
public sealed record UpdateAdminSellerStatusCommand(Guid SellerId, AdminUpdateSellerStatusRequestDto Request) : IRequest<AdminSellerDto>;
public sealed record GetAdminSellerStatsQuery(Guid SellerId) : IRequest<AdminSellerStatsDto?>;
public sealed record GetAdminProductsQuery(AdminProductQueryDto Query) : IRequest<PagedResultDto<AdminProductDto>>;
public sealed record UpdateAdminProductApprovalCommand(Guid ProductId, AdminUpdateProductApprovalRequestDto Request) : IRequest<AdminProductDto>;
public sealed record UpdateAdminProductVisibilityCommand(Guid ProductId, AdminUpdateProductVisibilityRequestDto Request) : IRequest<AdminProductDto>;
public sealed record DeleteAdminProductCommand(Guid ProductId) : IRequest<bool>;
public sealed record GetAdminOrdersQuery(AdminOrderQueryDto Query) : IRequest<PagedResultDto<AdminOrderListItemDto>>;
public sealed record GetAdminOrderByIdQuery(Guid OrderId) : IRequest<OrderDetailsDto?>;
public sealed record GetAdminRevenueQuery(string Period = "monthly") : IRequest<IReadOnlyList<AdminRevenuePointDto>>;
public sealed record GetAdminSalesQuery(string Period = "monthly") : IRequest<AdminSalesBreakdownDto>;
public sealed record GetAdminTopProductsQuery(int Take = 10) : IRequest<IReadOnlyList<AdminTopProductDto>>;
public sealed record GetAdminTopSellersQuery(int Take = 10) : IRequest<IReadOnlyList<AdminTopSellerDto>>;

public sealed class GetAdminDashboardOverviewHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetAdminDashboardOverviewQuery, AdminDashboardOverviewDto>
{
    public async Task<AdminDashboardOverviewDto> Handle(GetAdminDashboardOverviewQuery request, CancellationToken cancellationToken)
    {
        var users = (await unitOfWork.Users.GetAllUsersAsync()).ToList();
        var products = (await unitOfWork.Product.GetAllProductsAsync()).ToList();
        var orders = await unitOfWork.Orders.GetAllAsync(1, 50, cancellationToken);
        var allOrders = orders.ToList();

        var topProducts = AdminMappings.MapTopProducts(products, 5);
        var topSellers = AdminMappings.MapTopSellers(users, products, allOrders, 5);
        var recentOrders = allOrders
            .OrderByDescending(o => o.CreatedAt)
            .Take(5)
            .Select(AdminMappings.MapOrder)
            .ToList();
        var recentUsers = users
            .OrderByDescending(u => u.CreatedAt)
            .Take(5)
            .Select(AdminMappings.MapUser)
            .ToList();

        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        return new AdminDashboardOverviewDto(
            users.Count,
            users.Count(u => u.Role == UserRole.Customer),
            users.Count(u => u.Role == UserRole.Seller),
            products.Count,
            products.Count(p => p.ApprovalStatus == ApprovalStatus.Pending),
            allOrders.Count,
            allOrders.Count(o => o.Status == OrderStatus.Pending),
            allOrders.Count(o => o.Status == OrderStatus.Delivered),
            allOrders.Where(AdminMappings.IsRevenueOrder).Sum(o => o.TotalAmount),
            allOrders.Where(o => AdminMappings.IsRevenueOrder(o) && o.CreatedAt >= monthStart).Sum(o => o.TotalAmount),
            topProducts,
            topSellers,
            recentOrders,
            recentUsers);
    }
}

public sealed class GetAdminUsersHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetAdminUsersQuery, PagedResultDto<AdminUserDto>>
{
    public async Task<PagedResultDto<AdminUserDto>> Handle(GetAdminUsersQuery request, CancellationToken cancellationToken)
    {
        var query = (await unitOfWork.Users.GetAllUsersAsync()).AsQueryable();
        var filter = request.Query;

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            query = query.Where(u =>
                u.FullName.Contains(filter.Search, StringComparison.OrdinalIgnoreCase)
                || u.Email.Contains(filter.Search, StringComparison.OrdinalIgnoreCase));
        }

        if (filter.Role.HasValue)
        {
            query = query.Where(u => u.Role == filter.Role);
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(u => u.Status == filter.Status);
        }

        return AdminMappings.Page(
            query.OrderByDescending(u => u.CreatedAt).Select(AdminMappings.MapUser),
            filter.Page,
            filter.PageSize);
    }
}

public sealed class GetAdminUserByIdHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetAdminUserByIdQuery, AdminUserDto?>
{
    public async Task<AdminUserDto?> Handle(GetAdminUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await unitOfWork.Users.GetUserByIdAsync(request.UserId);
        return user is null ? null : AdminMappings.MapUser(user);
    }
}

public sealed class UpdateAdminUserStatusHandler(IUnitOfWork unitOfWork) : IRequestHandler<UpdateAdminUserStatusCommand, AdminUserDto>
{
    public async Task<AdminUserDto> Handle(UpdateAdminUserStatusCommand request, CancellationToken cancellationToken)
    {
        var user = await unitOfWork.Users.GetUserByIdAsync(request.UserId)
            ?? throw new KeyNotFoundException($"User '{request.UserId}' was not found.");

        user.Status = request.Request.Status;
        user.UpdatedAt = DateTime.UtcNow;
        await unitOfWork.Users.UpdateUserAsync(user);
        return AdminMappings.MapUser(user);
    }
}

public sealed class UpdateAdminUserRoleHandler(IUnitOfWork unitOfWork) : IRequestHandler<UpdateAdminUserRoleCommand, AdminUserDto>
{
    public async Task<AdminUserDto> Handle(UpdateAdminUserRoleCommand request, CancellationToken cancellationToken)
    {
        var user = await unitOfWork.Users.GetUserByIdAsync(request.UserId)
            ?? throw new KeyNotFoundException($"User '{request.UserId}' was not found.");

        user.Role = request.Request.Role;
        user.UpdatedAt = DateTime.UtcNow;

        if (request.Request.Role == UserRole.Seller && user.SellerProfile is null)
        {
            user.SellerProfile = new SellerProfile
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                ShopName = $"{user.FullName}'s Shop",
                ContactEmail = user.Email,
                Status = SellerStatus.Pending
            };
        }

        await unitOfWork.Users.UpdateUserAsync(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return AdminMappings.MapUser(user);
    }
}

public sealed class DeleteAdminUserHandler(IUnitOfWork unitOfWork) : IRequestHandler<DeleteAdminUserCommand, bool>
{
    public async Task<bool> Handle(DeleteAdminUserCommand request, CancellationToken cancellationToken)
    {
        var user = await unitOfWork.Users.GetUserByIdAsync(request.UserId);
        if (user is null)
        {
            return false;
        }

        user.DeletedAt = DateTime.UtcNow;
        user.Status = UserStatus.Inactive;
        user.UpdatedAt = DateTime.UtcNow;
        await unitOfWork.Users.UpdateUserAsync(user);
        return true;
    }
}

public sealed class GetAdminSellersHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetAdminSellersQuery, PagedResultDto<AdminSellerDto>>
{
    public async Task<PagedResultDto<AdminSellerDto>> Handle(GetAdminSellersQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Query;
        var sellers = (await unitOfWork.Users.GetAllUsersAsync())
            .Where(u => u.Role == UserRole.Seller && u.SellerProfile is not null)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            sellers = sellers.Where(u =>
                u.FullName.Contains(filter.Search, StringComparison.OrdinalIgnoreCase)
                || u.Email.Contains(filter.Search, StringComparison.OrdinalIgnoreCase)
                || (u.SellerProfile != null && u.SellerProfile.ShopName.Contains(filter.Search, StringComparison.OrdinalIgnoreCase)));
        }

        if (filter.Status.HasValue)
        {
            sellers = sellers.Where(u => u.SellerProfile!.Status == filter.Status.Value);
        }

        return AdminMappings.Page(
            sellers.OrderByDescending(u => u.CreatedAt).Select(AdminMappings.MapSeller),
            filter.Page,
            filter.PageSize);
    }
}

public sealed class GetAdminSellerByIdHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetAdminSellerByIdQuery, AdminSellerDto?>
{
    public async Task<AdminSellerDto?> Handle(GetAdminSellerByIdQuery request, CancellationToken cancellationToken)
    {
        var seller = await unitOfWork.Users.GetUserBySellerIdAsync(request.SellerId);
        return seller?.SellerProfile is null ? null : AdminMappings.MapSeller(seller);
    }
}

public sealed class UpdateAdminSellerStatusHandler(IUnitOfWork unitOfWork) : IRequestHandler<UpdateAdminSellerStatusCommand, AdminSellerDto>
{
    public async Task<AdminSellerDto> Handle(UpdateAdminSellerStatusCommand request, CancellationToken cancellationToken)
    {
        var seller = await unitOfWork.Users.GetUserBySellerIdAsync(request.SellerId)
            ?? throw new KeyNotFoundException($"Seller '{request.SellerId}' was not found.");
        var profile = seller.SellerProfile ?? throw new KeyNotFoundException($"Seller profile for '{request.SellerId}' was not found.");

        profile.Status = request.Request.Status;
        profile.UpdatedAt = DateTime.UtcNow;
        await unitOfWork.Users.UpdateUserAsync(seller);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return AdminMappings.MapSeller(seller);
    }
}

public sealed class GetAdminSellerStatsHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetAdminSellerStatsQuery, AdminSellerStatsDto?>
{
    public async Task<AdminSellerStatsDto?> Handle(GetAdminSellerStatsQuery request, CancellationToken cancellationToken)
    {
        var seller = await unitOfWork.Users.GetUserBySellerIdAsync(request.SellerId);
        if (seller?.SellerProfile is null)
        {
            return null;
        }

        var products = (await unitOfWork.Product.GetProductsBySellerIdAsync(request.SellerId)).ToList();
        var orders = (await unitOfWork.Orders.GetAllAsync(1, 1000, cancellationToken))
            .Where(o => o.OrderItems.Any(i => i.SellerId == request.SellerId))
            .ToList();

        return new AdminSellerStatsDto(
            seller.Id,
            seller.SellerProfile.ShopName,
            products.Count,
            products.Count(p => p.ApprovalStatus == ApprovalStatus.Approved),
            products.Count(p => p.ApprovalStatus == ApprovalStatus.Pending),
            orders.Count,
            orders.Count(o => o.Status == OrderStatus.Delivered),
            orders.Where(AdminMappings.IsRevenueOrder).Sum(o => o.OrderItems.Where(i => i.SellerId == request.SellerId).Sum(i => i.TotalPrice)));
    }
}

public sealed class GetAdminProductsHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetAdminProductsQuery, PagedResultDto<AdminProductDto>>
{
    public async Task<PagedResultDto<AdminProductDto>> Handle(GetAdminProductsQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Query;
        var query = (await unitOfWork.Product.GetAllProductsAsync()).AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            query = query.Where(p =>
                p.Name.Contains(filter.Search, StringComparison.OrdinalIgnoreCase)
                || (p.Seller != null && p.Seller.FullName.Contains(filter.Search, StringComparison.OrdinalIgnoreCase))
                || (p.Seller != null && p.Seller.SellerProfile != null && p.Seller.SellerProfile.ShopName.Contains(filter.Search, StringComparison.OrdinalIgnoreCase)));
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(p => p.ApprovalStatus == filter.Status.Value);
        }

        return AdminMappings.Page(
            query.OrderByDescending(p => p.CreatedAt).Select(AdminMappings.MapProduct),
            filter.Page,
            filter.PageSize);
    }
}

public sealed class UpdateAdminProductApprovalHandler(IUnitOfWork unitOfWork) : IRequestHandler<UpdateAdminProductApprovalCommand, AdminProductDto>
{
    public async Task<AdminProductDto> Handle(UpdateAdminProductApprovalCommand request, CancellationToken cancellationToken)
    {
        var product = await unitOfWork.Product.GetProductByIdAsync(request.ProductId)
            ?? throw new KeyNotFoundException($"Product '{request.ProductId}' was not found.");

        product.ApprovalStatus = request.Request.Status;
        product.AdminNotes = request.Request.AdminNotes;
        product.UpdatedAt = DateTime.UtcNow;
        await unitOfWork.Product.UpdateProductAsync(product);
        return AdminMappings.MapProduct(product);
    }
}

public sealed class UpdateAdminProductVisibilityHandler(IUnitOfWork unitOfWork) : IRequestHandler<UpdateAdminProductVisibilityCommand, AdminProductDto>
{
    public async Task<AdminProductDto> Handle(UpdateAdminProductVisibilityCommand request, CancellationToken cancellationToken)
    {
        var product = await unitOfWork.Product.GetProductByIdAsync(request.ProductId)
            ?? throw new KeyNotFoundException($"Product '{request.ProductId}' was not found.");

        product.IsActive = request.Request.IsActive;
        product.UpdatedAt = DateTime.UtcNow;
        await unitOfWork.Product.UpdateProductAsync(product);
        return AdminMappings.MapProduct(product);
    }
}

public sealed class DeleteAdminProductHandler(IUnitOfWork unitOfWork) : IRequestHandler<DeleteAdminProductCommand, bool>
{
    public async Task<bool> Handle(DeleteAdminProductCommand request, CancellationToken cancellationToken)
    {
        var product = await unitOfWork.Product.GetProductByIdAsync(request.ProductId);
        if (product is null)
        {
            return false;
        }

        product.IsActive = false;
        product.DeletedAt = DateTime.UtcNow;
        product.UpdatedAt = DateTime.UtcNow;
        await unitOfWork.Product.UpdateProductAsync(product);
        return true;
    }
}

public sealed class GetAdminOrdersHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetAdminOrdersQuery, PagedResultDto<AdminOrderListItemDto>>
{
    public async Task<PagedResultDto<AdminOrderListItemDto>> Handle(GetAdminOrdersQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Query;
        var allOrders = await unitOfWork.Orders.GetAllAsync(1, 5000, cancellationToken);
        var query = allOrders.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            query = query.Where(o =>
                o.Id.ToString().Contains(filter.Search, StringComparison.OrdinalIgnoreCase)
                || (o.User != null && o.User.FullName.Contains(filter.Search, StringComparison.OrdinalIgnoreCase))
                || (o.User != null && o.User.Email.Contains(filter.Search, StringComparison.OrdinalIgnoreCase)));
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(o => o.Status == filter.Status.Value);
        }

        return AdminMappings.Page(
            query.OrderByDescending(o => o.CreatedAt).Select(AdminMappings.MapOrder),
            filter.Page,
            filter.PageSize);
    }
}

public sealed class GetAdminOrderByIdHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetAdminOrderByIdQuery, OrderDetailsDto?>
{
    public async Task<OrderDetailsDto?> Handle(GetAdminOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await unitOfWork.Orders.GetByIdAsync(request.OrderId, cancellationToken);
        return order?.ToDetailsDto();
    }
}

public sealed class GetAdminRevenueHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetAdminRevenueQuery, IReadOnlyList<AdminRevenuePointDto>>
{
    public async Task<IReadOnlyList<AdminRevenuePointDto>> Handle(GetAdminRevenueQuery request, CancellationToken cancellationToken)
    {
        var orders = await unitOfWork.Orders.GetAllAsync(1, 5000, cancellationToken);
        return AdminMappings.BuildRevenueSeries(orders, request.Period);
    }
}

public sealed class GetAdminSalesHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetAdminSalesQuery, AdminSalesBreakdownDto>
{
    public async Task<AdminSalesBreakdownDto> Handle(GetAdminSalesQuery request, CancellationToken cancellationToken)
    {
        var orders = await unitOfWork.Orders.GetAllAsync(1, 5000, cancellationToken);
        var revenue = AdminMappings.BuildRevenueSeries(orders, request.Period);
        var salesByStatus = orders
            .GroupBy(o => o.Status.ToString())
            .Select(g => new KeyValuePair<string, decimal>(g.Key, g.Sum(x => x.TotalAmount)))
            .OrderByDescending(x => x.Value)
            .ToList();

        return new AdminSalesBreakdownDto(revenue, salesByStatus);
    }
}

public sealed class GetAdminTopProductsHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetAdminTopProductsQuery, IReadOnlyList<AdminTopProductDto>>
{
    public async Task<IReadOnlyList<AdminTopProductDto>> Handle(GetAdminTopProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await unitOfWork.Product.GetAllProductsAsync();
        return AdminMappings.MapTopProducts(products, request.Take);
    }
}

public sealed class GetAdminTopSellersHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetAdminTopSellersQuery, IReadOnlyList<AdminTopSellerDto>>
{
    public async Task<IReadOnlyList<AdminTopSellerDto>> Handle(GetAdminTopSellersQuery request, CancellationToken cancellationToken)
    {
        var users = (await unitOfWork.Users.GetAllUsersAsync()).ToList();
        var products = (await unitOfWork.Product.GetAllProductsAsync()).ToList();
        var orders = await unitOfWork.Orders.GetAllAsync(1, 5000, cancellationToken);
        return AdminMappings.MapTopSellers(users, products, orders, request.Take);
    }
}

internal static class AdminMappings
{
    public static bool IsRevenueOrder(OrderEntity order) => order.Status is OrderStatus.Confirmed or OrderStatus.Processing or OrderStatus.Shipped or OrderStatus.Delivered;

    public static AdminUserDto MapUser(UserEntity user) => new(
        user.Id,
        user.KeycloakId,
        user.Email,
        user.FullName,
        user.Role,
        user.Status,
        user.CreatedAt,
        user.UpdatedAt,
        user.DeletedAt);

    public static AdminSellerDto MapSeller(UserEntity seller)
    {
        var profile = seller.SellerProfile ?? throw new InvalidOperationException("Seller profile is required.");
        return new AdminSellerDto(
            seller.Id,
            profile.Id,
            seller.FullName,
            seller.Email,
            profile.ShopName,
            profile.Status,
            profile.TotalSales,
            profile.TotalRevenue,
            profile.Rating,
            profile.CommissionRate,
            profile.CreatedAt);
    }

    public static AdminProductDto MapProduct(Shopbe.Domain.Entities.Product.Product product) => new(
        product.Id,
        product.Name,
        product.Slug,
        product.Seller?.FullName ?? string.Empty,
        product.Seller?.SellerProfile?.ShopName,
        product.Category?.Name,
        product.BasePrice,
        product.ApprovalStatus,
        product.IsActive,
        product.AdminNotes,
        product.CreatedAt);

    public static AdminOrderListItemDto MapOrder(OrderEntity order) => new(
        order.Id,
        order.UserId,
        order.User?.FullName ?? string.Empty,
        order.User?.Email ?? string.Empty,
        order.TotalAmount,
        order.Currency,
        order.Status,
        order.OrderItems.Sum(i => i.Quantity),
        order.CreatedAt);

    public static PagedResultDto<T> Page<T>(IEnumerable<T> source, int page, int pageSize)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 20 : Math.Min(pageSize, 100);
        var list = source.ToList();
        return new PagedResultDto<T>
        {
            Items = list.Skip((page - 1) * pageSize).Take(pageSize).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = list.Count
        };
    }

    public static IReadOnlyList<AdminTopProductDto> MapTopProducts(IEnumerable<Shopbe.Domain.Entities.Product.Product> products, int take)
    {
        return products
            .OrderByDescending(p => p.SoldCount)
            .ThenByDescending(p => p.CreatedAt)
            .Take(Math.Max(take, 1))
            .Select(p => new AdminTopProductDto(
                p.Id,
                p.Name,
                p.SoldCount,
                p.OrderItems.Sum(i => i.TotalPrice)))
            .ToList();
    }

    public static IReadOnlyList<AdminTopSellerDto> MapTopSellers(
        IReadOnlyList<UserEntity> users,
        IReadOnlyList<Shopbe.Domain.Entities.Product.Product> products,
        IReadOnlyList<OrderEntity> orders,
        int take)
    {
        return users
            .Where(u => u.Role == UserRole.Seller)
            .Select(seller =>
            {
                var sellerProducts = products.Where(p => p.SellerId == seller.Id).ToList();
                var sellerRevenue = orders
                    .Where(IsRevenueOrder)
                    .SelectMany(o => o.OrderItems)
                    .Where(i => i.SellerId == seller.Id)
                    .Sum(i => i.TotalPrice);

                return new AdminTopSellerDto(
                    seller.Id,
                    seller.FullName,
                    seller.SellerProfile?.ShopName,
                    sellerProducts.Sum(p => p.SoldCount),
                    sellerRevenue);
            })
            .OrderByDescending(x => x.Revenue)
            .ThenByDescending(x => x.TotalSales)
            .Take(Math.Max(take, 1))
            .ToList();
    }

    public static IReadOnlyList<AdminRevenuePointDto> BuildRevenueSeries(IEnumerable<OrderEntity> orders, string period)
    {
        var normalized = string.IsNullOrWhiteSpace(period) ? "monthly" : period.Trim().ToLowerInvariant();
        return normalized switch
        {
            "daily" => orders.Where(IsRevenueOrder)
                .GroupBy(o => o.CreatedAt.ToString("yyyy-MM-dd"))
                .OrderBy(g => g.Key)
                .Select(g => new AdminRevenuePointDto(g.Key, g.Sum(o => o.TotalAmount), g.Count()))
                .ToList(),
            "weekly" => orders.Where(IsRevenueOrder)
                .GroupBy(o => $"{o.CreatedAt:yyyy}-W{System.Globalization.ISOWeek.GetWeekOfYear(o.CreatedAt):00}")
                .OrderBy(g => g.Key)
                .Select(g => new AdminRevenuePointDto(g.Key, g.Sum(o => o.TotalAmount), g.Count()))
                .ToList(),
            "yearly" => orders.Where(IsRevenueOrder)
                .GroupBy(o => o.CreatedAt.Year.ToString())
                .OrderBy(g => g.Key)
                .Select(g => new AdminRevenuePointDto(g.Key, g.Sum(o => o.TotalAmount), g.Count()))
                .ToList(),
            _ => orders.Where(IsRevenueOrder)
                .GroupBy(o => o.CreatedAt.ToString("yyyy-MM"))
                .OrderBy(g => g.Key)
                .Select(g => new AdminRevenuePointDto(g.Key, g.Sum(o => o.TotalAmount), g.Count()))
                .ToList()
        };
    }
}
