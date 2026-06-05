using MediatR;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.Order;
using Shopbe.Application.Order.Dtos;
using Shopbe.Application.Product.ProductImages.Dtos;
using Shopbe.Application.Product.ProductVariants.Dtos;
using Shopbe.Application.Seller.Dtos;
using Shopbe.Domain.Entities.Seller;
using Shopbe.Domain.Enums;
using OrderEntity = Shopbe.Domain.Entities.Order.Order;
using OrderStatusHistoryEntity = Shopbe.Domain.Entities.Order.OrderStatusHistory;
using ProductEntity = Shopbe.Domain.Entities.Product.Product;
using ProductImageEntity = Shopbe.Domain.Entities.Product.ProductImage;
using ProductVariantEntity = Shopbe.Domain.Entities.Product.ProductVariant;
using UserEntity = Shopbe.Domain.Entities.User.User;

namespace Shopbe.Application.Seller;

public sealed record GetSellerDashboardOverviewQuery() : IRequest<SellerDashboardOverviewDto>;
public sealed record GetSellerProductsQuery(SellerProductQueryDto Query) : IRequest<PagedResultDto<SellerProductListItemDto>>;
public sealed record CreateSellerProductCommand(SellerProductUpsertDto Request) : IRequest<SellerProductListItemDto>;
public sealed record UpdateSellerProductCommand(Guid ProductId, SellerProductUpsertDto Request) : IRequest<SellerProductListItemDto>;
public sealed record DeleteSellerProductCommand(Guid ProductId) : IRequest<bool>;
public sealed record GetSellerOrdersQuery(SellerOrderQueryDto Query) : IRequest<PagedResultDto<SellerOrderListItemDto>>;
public sealed record GetSellerOrderByIdQuery(Guid OrderId) : IRequest<OrderDetailsDto?>;
public sealed record UpdateSellerOrderStatusCommand(Guid OrderId, SellerUpdateOrderStatusRequestDto Request) : IRequest<OrderDetailsDto>;
public sealed record GetSellerRevenueQuery(string Period = "monthly") : IRequest<IReadOnlyList<SellerRevenuePointDto>>;
public sealed record GetSellerSalesQuery(string Period = "monthly") : IRequest<IReadOnlyList<SellerRevenuePointDto>>;
public sealed record GetSellerLowStockProductsQuery(int Threshold = 10) : IRequest<IReadOnlyList<SellerLowStockProductDto>>;
public sealed record GetSellerProfileQuery() : IRequest<SellerProfileDto?>;
public sealed record UpdateSellerProfileCommand(SellerProfileUpsertDto Request) : IRequest<SellerProfileDto>;

public sealed class GetSellerDashboardOverviewHandler(IUnitOfWork unitOfWork, ICurrentUser currentUser)
    : IRequestHandler<GetSellerDashboardOverviewQuery, SellerDashboardOverviewDto>
{
    public async Task<SellerDashboardOverviewDto> Handle(GetSellerDashboardOverviewQuery request, CancellationToken cancellationToken)
    {
        var seller = await SellerSupport.RequireSellerAsync(unitOfWork, currentUser);
        var products = (await unitOfWork.Product.GetProductsBySellerIdAsync(seller.Id)).ToList();
        var orders = (await unitOfWork.Orders.GetAllAsync(1, 5000, cancellationToken))
            .Where(o => o.OrderItems.Any(i => i.SellerId == seller.Id))
            .ToList();

        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        return new SellerDashboardOverviewDto(
            products.Count,
            orders.Count(o => o.Status == OrderStatus.Pending),
            orders.Count(o => o.Status == OrderStatus.Processing),
            orders.Count(o => o.Status == OrderStatus.Shipped),
            orders.Count(o => o.Status == OrderStatus.Delivered),
            SellerSupport.CalculateSellerRevenue(orders, seller.Id),
            SellerSupport.CalculateSellerRevenue(orders.Where(o => o.CreatedAt >= monthStart), seller.Id),
            SellerSupport.MapLowStockProducts(products, 10),
            orders.OrderByDescending(o => o.CreatedAt).Take(5).Select(o => SellerSupport.MapOrder(o, seller.Id)).ToList(),
            products.OrderByDescending(p => p.SoldCount).Take(5).Select(SellerSupport.MapProduct).ToList());
    }
}

public sealed class GetSellerProductsHandler(IUnitOfWork unitOfWork, ICurrentUser currentUser)
    : IRequestHandler<GetSellerProductsQuery, PagedResultDto<SellerProductListItemDto>>
{
    public async Task<PagedResultDto<SellerProductListItemDto>> Handle(GetSellerProductsQuery request, CancellationToken cancellationToken)
    {
        var seller = await SellerSupport.RequireSellerAsync(unitOfWork, currentUser);
        var filter = request.Query;
        var query = (await unitOfWork.Product.GetProductsBySellerIdAsync(seller.Id)).AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            query = query.Where(p => p.Name.Contains(filter.Search, StringComparison.OrdinalIgnoreCase));
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(p => p.ApprovalStatus == filter.Status.Value);
        }

        return SellerSupport.Page(
            query.OrderByDescending(p => p.CreatedAt).Select(SellerSupport.MapProduct),
            filter.Page,
            filter.PageSize);
    }
}

public sealed class CreateSellerProductHandler(IUnitOfWork unitOfWork, ICurrentUser currentUser)
    : IRequestHandler<CreateSellerProductCommand, SellerProductListItemDto>
{
    public async Task<SellerProductListItemDto> Handle(CreateSellerProductCommand request, CancellationToken cancellationToken)
    {
        var seller = await SellerSupport.RequireSellerAsync(unitOfWork, currentUser);
        await SellerSupport.ValidateProductRequestAsync(unitOfWork, request.Request, cancellationToken);

        var product = new ProductEntity
        {
            Id = Guid.NewGuid(),
            Name = request.Request.Name,
            Slug = SellerSupport.GenerateSlug(request.Request.Name),
            Description = request.Request.Description,
            BasePrice = request.Request.BasePrice,
            CategoryId = request.Request.CategoryId,
            BrandId = request.Request.BrandId,
            IsActive = request.Request.IsActive,
            SellerId = seller.Id,
            ApprovalStatus = ApprovalStatus.Pending,
            AdminNotes = null,
            Images = SellerSupport.MapImages(request.Request.Images),
            Variants = SellerSupport.MapVariants(request.Request.Variants)
        };

        foreach (var image in product.Images)
        {
            image.ProductId = product.Id;
        }

        foreach (var variant in product.Variants)
        {
            variant.ProductId = product.Id;
        }

        await unitOfWork.Product.AddProductAsync(product);
        return SellerSupport.MapProduct(product);
    }
}

public sealed class UpdateSellerProductHandler(IUnitOfWork unitOfWork, ICurrentUser currentUser)
    : IRequestHandler<UpdateSellerProductCommand, SellerProductListItemDto>
{
    public async Task<SellerProductListItemDto> Handle(UpdateSellerProductCommand request, CancellationToken cancellationToken)
    {
        var seller = await SellerSupport.RequireSellerAsync(unitOfWork, currentUser);
        await SellerSupport.ValidateProductRequestAsync(unitOfWork, request.Request, cancellationToken);

        var product = await unitOfWork.Product.GetProductByIdAsync(request.ProductId)
            ?? throw new KeyNotFoundException($"Product '{request.ProductId}' was not found.");

        if (product.SellerId != seller.Id)
        {
            throw new UnauthorizedAccessException("You can only update your own products.");
        }

        product.Name = request.Request.Name;
        product.Slug = SellerSupport.GenerateSlug(request.Request.Name);
        product.Description = request.Request.Description;
        product.BasePrice = request.Request.BasePrice;
        product.CategoryId = request.Request.CategoryId;
        product.BrandId = request.Request.BrandId;
        product.IsActive = request.Request.IsActive;
        product.ApprovalStatus = ApprovalStatus.Pending;
        product.AdminNotes = null;
        product.UpdatedAt = DateTime.UtcNow;

        product.Images.Clear();
        foreach (var image in SellerSupport.MapImages(request.Request.Images))
        {
            image.ProductId = product.Id;
            product.Images.Add(image);
        }

        product.Variants.Clear();
        foreach (var variant in SellerSupport.MapVariants(request.Request.Variants))
        {
            variant.ProductId = product.Id;
            product.Variants.Add(variant);
        }

        await unitOfWork.Product.UpdateProductAsync(product);
        return SellerSupport.MapProduct(product);
    }
}

public sealed class DeleteSellerProductHandler(IUnitOfWork unitOfWork, ICurrentUser currentUser)
    : IRequestHandler<DeleteSellerProductCommand, bool>
{
    public async Task<bool> Handle(DeleteSellerProductCommand request, CancellationToken cancellationToken)
    {
        var seller = await SellerSupport.RequireSellerAsync(unitOfWork, currentUser);
        var product = await unitOfWork.Product.GetProductByIdAsync(request.ProductId);
        if (product is null || product.SellerId != seller.Id)
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

public sealed class GetSellerOrdersHandler(IUnitOfWork unitOfWork, ICurrentUser currentUser)
    : IRequestHandler<GetSellerOrdersQuery, PagedResultDto<SellerOrderListItemDto>>
{
    public async Task<PagedResultDto<SellerOrderListItemDto>> Handle(GetSellerOrdersQuery request, CancellationToken cancellationToken)
    {
        var seller = await SellerSupport.RequireSellerAsync(unitOfWork, currentUser);
        var filter = request.Query;
        var query = (await unitOfWork.Orders.GetAllAsync(1, 5000, cancellationToken))
            .Where(o => o.OrderItems.Any(i => i.SellerId == seller.Id))
            .AsQueryable();

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

        return SellerSupport.Page(
            query.OrderByDescending(o => o.CreatedAt).Select(o => SellerSupport.MapOrder(o, seller.Id)),
            filter.Page,
            filter.PageSize);
    }
}

public sealed class GetSellerOrderByIdHandler(IUnitOfWork unitOfWork, ICurrentUser currentUser)
    : IRequestHandler<GetSellerOrderByIdQuery, OrderDetailsDto?>
{
    public async Task<OrderDetailsDto?> Handle(GetSellerOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var seller = await SellerSupport.RequireSellerAsync(unitOfWork, currentUser);
        var order = await unitOfWork.Orders.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null || order.OrderItems.All(i => i.SellerId != seller.Id))
        {
            return null;
        }

        return order.ToDetailsDto();
    }
}

public sealed class UpdateSellerOrderStatusHandler(IUnitOfWork unitOfWork, ICurrentUser currentUser)
    : IRequestHandler<UpdateSellerOrderStatusCommand, OrderDetailsDto>
{
    public async Task<OrderDetailsDto> Handle(UpdateSellerOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var seller = await SellerSupport.RequireSellerAsync(unitOfWork, currentUser);
        var order = await unitOfWork.Orders.GetByIdAsync(request.OrderId, cancellationToken)
            ?? throw new KeyNotFoundException($"Order '{request.OrderId}' was not found.");

        if (order.OrderItems.All(i => i.SellerId != seller.Id))
        {
            throw new UnauthorizedAccessException("You can only update orders containing your products.");
        }

        if (order.OrderItems.Any(i => i.SellerId != seller.Id))
        {
            throw new InvalidOperationException("Seller status updates are only supported for single-seller orders with the current schema.");
        }

        if (request.Request.Status is not (OrderStatus.Processing or OrderStatus.Shipped or OrderStatus.Delivered))
        {
            throw new ArgumentException("Seller can only set status to Processing, Shipped, or Delivered.");
        }

        order.Status = request.Request.Status;
        order.UpdatedAt = DateTime.UtcNow;
        order.OrderStatusHistory.Add(new OrderStatusHistoryEntity
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            Status = request.Request.Status,
            Note = request.Request.Note,
            ChangedBy = seller.Id,
            ChangedAt = DateTime.UtcNow
        });

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return order.ToDetailsDto();
    }
}

public sealed class GetSellerRevenueHandler(IUnitOfWork unitOfWork, ICurrentUser currentUser)
    : IRequestHandler<GetSellerRevenueQuery, IReadOnlyList<SellerRevenuePointDto>>
{
    public async Task<IReadOnlyList<SellerRevenuePointDto>> Handle(GetSellerRevenueQuery request, CancellationToken cancellationToken)
    {
        var seller = await SellerSupport.RequireSellerAsync(unitOfWork, currentUser);
        var orders = (await unitOfWork.Orders.GetAllAsync(1, 5000, cancellationToken))
            .Where(o => o.OrderItems.Any(i => i.SellerId == seller.Id))
            .ToList();
        return SellerSupport.BuildRevenueSeries(orders, seller.Id, request.Period);
    }
}

public sealed class GetSellerSalesHandler(IUnitOfWork unitOfWork, ICurrentUser currentUser)
    : IRequestHandler<GetSellerSalesQuery, IReadOnlyList<SellerRevenuePointDto>>
{
    public async Task<IReadOnlyList<SellerRevenuePointDto>> Handle(GetSellerSalesQuery request, CancellationToken cancellationToken)
    {
        var seller = await SellerSupport.RequireSellerAsync(unitOfWork, currentUser);
        var orders = (await unitOfWork.Orders.GetAllAsync(1, 5000, cancellationToken))
            .Where(o => o.OrderItems.Any(i => i.SellerId == seller.Id))
            .ToList();
        return SellerSupport.BuildRevenueSeries(orders, seller.Id, request.Period);
    }
}

public sealed class GetSellerLowStockProductsHandler(IUnitOfWork unitOfWork, ICurrentUser currentUser)
    : IRequestHandler<GetSellerLowStockProductsQuery, IReadOnlyList<SellerLowStockProductDto>>
{
    public async Task<IReadOnlyList<SellerLowStockProductDto>> Handle(GetSellerLowStockProductsQuery request, CancellationToken cancellationToken)
    {
        var seller = await SellerSupport.RequireSellerAsync(unitOfWork, currentUser);
        var products = await unitOfWork.Product.GetProductsBySellerIdAsync(seller.Id);
        return SellerSupport.MapLowStockProducts(products, request.Threshold);
    }
}

public sealed class GetSellerProfileHandler(IUnitOfWork unitOfWork, ICurrentUser currentUser)
    : IRequestHandler<GetSellerProfileQuery, SellerProfileDto?>
{
    public async Task<SellerProfileDto?> Handle(GetSellerProfileQuery request, CancellationToken cancellationToken)
    {
        var seller = await SellerSupport.RequireSellerAsync(unitOfWork, currentUser);
        return seller.SellerProfile is null ? null : SellerSupport.MapProfile(seller.SellerProfile);
    }
}

public sealed class UpdateSellerProfileHandler(IUnitOfWork unitOfWork, ICurrentUser currentUser)
    : IRequestHandler<UpdateSellerProfileCommand, SellerProfileDto>
{
    public async Task<SellerProfileDto> Handle(UpdateSellerProfileCommand request, CancellationToken cancellationToken)
    {
        var seller = await SellerSupport.RequireSellerAsync(unitOfWork, currentUser);
        var profile = seller.SellerProfile ?? new SellerProfile
        {
            Id = Guid.NewGuid(),
            UserId = seller.Id,
            Status = SellerStatus.Pending,
            CommissionRate = 0.05m
        };

        profile.ShopName = request.Request.ShopName;
        profile.ShopDescription = request.Request.ShopDescription;
        profile.ShopLogoUrl = request.Request.ShopLogoUrl;
        profile.ShopBannerUrl = request.Request.ShopBannerUrl;
        profile.ContactPhone = request.Request.ContactPhone;
        profile.ContactEmail = request.Request.ContactEmail;
        profile.Address = request.Request.Address;
        profile.City = request.Request.City;
        profile.UpdatedAt = DateTime.UtcNow;

        seller.SellerProfile = profile;
        await unitOfWork.Users.UpdateUserAsync(seller);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return SellerSupport.MapProfile(profile);
    }
}

internal static class SellerSupport
{
    public static async Task<UserEntity> RequireSellerAsync(IUnitOfWork unitOfWork, ICurrentUser currentUser)
    {
        if (!currentUser.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("Missing authenticated user.");
        }

        var keycloakId = currentUser.KeycloakId;
        if (string.IsNullOrWhiteSpace(keycloakId))
        {
            throw new UnauthorizedAccessException("Missing user identity.");
        }

        var seller = await unitOfWork.Users.GetUserByKeycloakIdAsync(keycloakId)
            ?? throw new UnauthorizedAccessException("Seller account not found.");

        if (seller.Role != UserRole.Seller)
        {
            throw new UnauthorizedAccessException("Seller role is required.");
        }

        return seller;
    }

    public static async Task ValidateProductRequestAsync(IUnitOfWork unitOfWork, SellerProductUpsertDto request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ArgumentException("Product name is required.");
        }

        if (request.BasePrice < 0)
        {
            throw new ArgumentException("Product price cannot be negative.");
        }

        var category = await unitOfWork.Category.GetCategoryByIdAsync(request.CategoryId);
        if (category is null)
        {
            throw new KeyNotFoundException($"Category '{request.CategoryId}' was not found.");
        }

        ValidateImages(request.Images);
        ValidateVariants(request.Variants);
    }

    public static void ValidateImages(IEnumerable<ProductImageRequestDto>? images)
    {
        if (images is null)
        {
            return;
        }

        var imageList = images.ToList();
        if (imageList.Any(i => string.IsNullOrWhiteSpace(i.ImageUrl)))
        {
            throw new ArgumentException("Image URL is required for all product images.");
        }

        if (imageList.Count(i => i.IsPrimary) > 1)
        {
            throw new ArgumentException("Only one product image can be marked as primary.");
        }
    }

    public static void ValidateVariants(IEnumerable<ProductVariantRequestDto>? variants)
    {
        if (variants is null)
        {
            return;
        }

        var variantList = variants.ToList();
        if (variantList.Any(v => string.IsNullOrWhiteSpace(v.SKU)))
        {
            throw new ArgumentException("SKU is required for all variants.");
        }

        if (variantList.Any(v => v.Price < 0))
        {
            throw new ArgumentException("Variant price cannot be negative.");
        }

        if (variantList.Any(v => v.StockQuantity < 0))
        {
            throw new ArgumentException("Variant stock quantity cannot be negative.");
        }
    }

    public static string GenerateSlug(string value)
    {
        var source = value.Trim().ToLowerInvariant();
        var chars = source.Select(ch => char.IsLetterOrDigit(ch) ? ch : '-').ToArray();
        var slug = new string(chars);
        while (slug.Contains("--", StringComparison.Ordinal))
        {
            slug = slug.Replace("--", "-", StringComparison.Ordinal);
        }

        return slug.Trim('-');
    }

    public static List<ProductImageEntity> MapImages(IEnumerable<ProductImageRequestDto>? images)
    {
        return (images ?? Enumerable.Empty<ProductImageRequestDto>())
            .Select(image => new ProductImageEntity
            {
                Id = Guid.NewGuid(),
                ImageUrl = image.ImageUrl,
                IsPrimary = image.IsPrimary
            })
            .ToList();
    }

    public static List<ProductVariantEntity> MapVariants(IEnumerable<ProductVariantRequestDto>? variants)
    {
        return (variants ?? Enumerable.Empty<ProductVariantRequestDto>())
            .Select(variant => new ProductVariantEntity
            {
                Id = Guid.NewGuid(),
                Sku = variant.SKU,
                Price = variant.Price,
                StockQuantity = variant.StockQuantity,
                IsActive = variant.IsActive
            })
            .ToList();
    }

    public static SellerProductListItemDto MapProduct(ProductEntity product) => new(
        product.Id,
        product.Name,
        product.Slug,
        product.Category?.Name,
        product.BasePrice,
        product.Variants.Sum(v => v.StockQuantity),
        product.ApprovalStatus,
        product.IsActive,
        product.CreatedAt);

    public static SellerOrderListItemDto MapOrder(OrderEntity order, Guid sellerId) => new(
        order.Id,
        order.User?.FullName ?? string.Empty,
        order.User?.Email ?? string.Empty,
        order.OrderItems.Where(i => i.SellerId == sellerId).Sum(i => i.Quantity),
        order.OrderItems.Where(i => i.SellerId == sellerId).Sum(i => i.TotalPrice),
        order.Status,
        order.CreatedAt);

    public static SellerProfileDto MapProfile(SellerProfile profile) => new(
        profile.Id,
        profile.UserId,
        profile.ShopName,
        profile.ShopDescription,
        profile.ShopLogoUrl,
        profile.ShopBannerUrl,
        profile.ContactPhone,
        profile.ContactEmail,
        profile.Address,
        profile.City,
        profile.Status,
        profile.CommissionRate,
        profile.Rating,
        profile.TotalSales,
        profile.TotalRevenue);

    public static decimal CalculateSellerRevenue(IEnumerable<OrderEntity> orders, Guid sellerId)
    {
        return orders
            .Where(o => o.Status is OrderStatus.Confirmed or OrderStatus.Processing or OrderStatus.Shipped or OrderStatus.Delivered)
            .SelectMany(o => o.OrderItems)
            .Where(i => i.SellerId == sellerId)
            .Sum(i => i.TotalPrice);
    }

    public static IReadOnlyList<SellerLowStockProductDto> MapLowStockProducts(IEnumerable<ProductEntity> products, int threshold)
    {
        return products
            .Select(p => new SellerLowStockProductDto(p.Id, p.Name, p.Variants.Sum(v => v.StockQuantity)))
            .Where(p => p.Stock <= threshold)
            .OrderBy(p => p.Stock)
            .ToList();
    }

    public static IReadOnlyList<SellerRevenuePointDto> BuildRevenueSeries(IEnumerable<OrderEntity> orders, Guid sellerId, string period)
    {
        var normalized = string.IsNullOrWhiteSpace(period) ? "monthly" : period.Trim().ToLowerInvariant();
        var revenueOrders = orders.Where(o => o.Status is OrderStatus.Confirmed or OrderStatus.Processing or OrderStatus.Shipped or OrderStatus.Delivered);

        return normalized switch
        {
            "daily" => revenueOrders
                .GroupBy(o => o.CreatedAt.ToString("yyyy-MM-dd"))
                .OrderBy(g => g.Key)
                .Select(g => new SellerRevenuePointDto(g.Key, g.SelectMany(o => o.OrderItems).Where(i => i.SellerId == sellerId).Sum(i => i.TotalPrice), g.Count()))
                .ToList(),
            "weekly" => revenueOrders
                .GroupBy(o => $"{o.CreatedAt:yyyy}-W{System.Globalization.ISOWeek.GetWeekOfYear(o.CreatedAt):00}")
                .OrderBy(g => g.Key)
                .Select(g => new SellerRevenuePointDto(g.Key, g.SelectMany(o => o.OrderItems).Where(i => i.SellerId == sellerId).Sum(i => i.TotalPrice), g.Count()))
                .ToList(),
            "yearly" => revenueOrders
                .GroupBy(o => o.CreatedAt.Year.ToString())
                .OrderBy(g => g.Key)
                .Select(g => new SellerRevenuePointDto(g.Key, g.SelectMany(o => o.OrderItems).Where(i => i.SellerId == sellerId).Sum(i => i.TotalPrice), g.Count()))
                .ToList(),
            _ => revenueOrders
                .GroupBy(o => o.CreatedAt.ToString("yyyy-MM"))
                .OrderBy(g => g.Key)
                .Select(g => new SellerRevenuePointDto(g.Key, g.SelectMany(o => o.OrderItems).Where(i => i.SellerId == sellerId).Sum(i => i.TotalPrice), g.Count()))
                .ToList()
        };
    }

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
}
