using Microsoft.EntityFrameworkCore;
using Shopbe.Domain.Entities.Category;
using Shopbe.Domain.Entities.Order;
using Shopbe.Domain.Entities.Product;
using Shopbe.Domain.Entities.Seller;
using Shopbe.Domain.Entities.User;
using Shopbe.Domain.Enums;
using Shopbe.Infrastructure.Persistence;

namespace Shopbe.E2E.Tests.Infrastructure;

public static class DatabaseSeed
{
    public sealed record SeedResult(Guid CategoryId, Guid ProductId, Guid VariantId);
    public sealed record DemoDashboardSeedResult(
        Guid AdminUserId,
        Guid Seller1UserId,
        Guid Seller2UserId,
        Guid CustomerUserId,
        Guid DeliveredOrderId,
        Guid PendingOrderId,
        decimal DeliveredOrderRevenue,
        string CustomerEmail);

    public static async Task<SeedResult> SeedMinimalCatalogAsync(ShopDbContext db, CancellationToken ct = default)
    {
        // Ensure an admin user exists for SellerId FK
        var adminUser = await db.Users
            .OrderBy(u => u.CreatedAt)
            .FirstOrDefaultAsync(u => u.Role == UserRole.Admin, ct);
        if (adminUser is null)
        {
            adminUser = new User
            {
                KeycloakId = "e2e-admin",
                Email = "e2e-admin@test.com",
                FullName = "E2E Admin",
                Role = UserRole.Admin,
                Status = UserStatus.Active
            };
            db.Users.Add(adminUser);
            await db.SaveChangesAsync(ct);
        }

        // Ensure a category exists
        var category = await db.Categories
            .AsNoTracking()
            .OrderBy(c => c.CreatedAt)
            .FirstOrDefaultAsync(ct);
        if (category is null)
        {
            category = new Category
            {
                Name = "E2E Category",
                Slug = "e2e-category",
                IsActive = true
            };
            db.Categories.Add(category);
            await db.SaveChangesAsync(ct);
        }

        var product = new Product
        {
            Name = "E2E Product",
            Slug = $"e2e-product-{Guid.NewGuid():N}",
            Description = "Seeded for end-to-end tests",
            BasePrice = 100_000m,
            CategoryId = category.Id,
            IsActive = true,
            SellerId = adminUser.Id
        };
        db.Products.Add(product);
        await db.SaveChangesAsync(ct);

        var variant = new ProductVariant
        {
            ProductId = product.Id,
            Sku = $"E2E-SKU-{Guid.NewGuid():N}".Substring(0, 20),
            Price = 100_000m,
            StockQuantity = 10,
            IsActive = true
        };
        db.ProductVariants.Add(variant);
        await db.SaveChangesAsync(ct);

        return new SeedResult(category.Id, product.Id, variant.Id);
    }

    public static async Task<DemoDashboardSeedResult> SeedDemoDashboardScenarioAsync(ShopDbContext db, CancellationToken ct = default)
    {
        var admin = await EnsureUserAsync(db, "phase2-admin", "phase2-admin@test.local", "Phase 2 Admin", UserRole.Admin, ct);
        var seller1 = await EnsureUserAsync(db, "phase2-seller1", "phase2-seller1@test.local", "Phase 2 Seller One", UserRole.Seller, ct, "Phase 2 Seller One Shop");
        var seller2 = await EnsureUserAsync(db, "phase2-seller2", "phase2-seller2@test.local", "Phase 2 Seller Two", UserRole.Seller, ct, "Phase 2 Seller Two Shop");
        var customer = await EnsureUserAsync(db, "phase2-customer1", "phase2-customer1@test.local", "Phase 2 Customer", UserRole.Customer, ct);

        var address = customer.UserAddresses.FirstOrDefault(a => a.IsDefault) ?? customer.UserAddresses.First();

        var category = await db.Categories.FirstOrDefaultAsync(c => c.Slug == "phase2-dashboard-category", ct);
        if (category is null)
        {
            category = new Category
            {
                Name = "Phase 2 Dashboard Category",
                Slug = "phase2-dashboard-category",
                IsActive = true
            };
            db.Categories.Add(category);
            await db.SaveChangesAsync(ct);
        }

        var deliveredVariant = await EnsureProductVariantAsync(
            db,
            seller1.Id,
            category.Id,
            "phase2-seller1-headphones",
            "Phase 2 Seller 1 Headphones",
            "PHASE2-SELLER1-001",
            120_000m,
            ct);

        var pendingVariant = await EnsureProductVariantAsync(
            db,
            seller2.Id,
            category.Id,
            "phase2-seller2-stand",
            "Phase 2 Seller 2 Monitor Stand",
            "PHASE2-SELLER2-001",
            80_000m,
            ct);

        var deliveredOrder = await EnsureOrderAsync(
            db,
            note: "phase2-delivered-order",
            customer,
            address,
            seller1,
            deliveredVariant,
            OrderStatus.Delivered,
            shippingFee: 10_000m,
            quantity: 1,
            createdAt: DateTime.UtcNow.AddDays(-5),
            changedAt: DateTime.UtcNow.AddDays(-4),
            ct);

        var pendingOrder = await EnsureOrderAsync(
            db,
            note: "phase2-pending-order",
            customer,
            address,
            seller2,
            pendingVariant,
            OrderStatus.Pending,
            shippingFee: 5_000m,
            quantity: 2,
            createdAt: DateTime.UtcNow.AddDays(-1),
            changedAt: DateTime.UtcNow.AddDays(-1),
            ct);

        deliveredVariant.Product!.SoldCount = 1;
        pendingVariant.Product!.SoldCount = 2;

        seller1.SellerProfile!.TotalSales = 1;
        seller1.SellerProfile.TotalRevenue = 120_000m;
        seller2.SellerProfile!.TotalSales = 2;
        seller2.SellerProfile.TotalRevenue = 0m;

        await db.SaveChangesAsync(ct);

        return new DemoDashboardSeedResult(
            admin.Id,
            seller1.Id,
            seller2.Id,
            customer.Id,
            deliveredOrder.Id,
            pendingOrder.Id,
            120_000m,
            customer.Email);
    }

    private static async Task<User> EnsureUserAsync(
        ShopDbContext db,
        string keycloakId,
        string email,
        string fullName,
        UserRole role,
        CancellationToken ct,
        string? shopName = null)
    {
        var user = await db.Users
            .Include(u => u.UserAddresses)
            .Include(u => u.SellerProfile)
            .FirstOrDefaultAsync(u => u.Email == email, ct);

        if (user is null)
        {
            user = new User
            {
                KeycloakId = keycloakId,
                Email = email,
                FullName = fullName,
                Role = role,
                Status = UserStatus.Active,
                Country = "Vietnam",
                Language = "vi-VN"
            };
            db.Users.Add(user);
            await db.SaveChangesAsync(ct);
        }
        else
        {
            user.KeycloakId = keycloakId;
            user.FullName = fullName;
            user.Role = role;
            user.Status = UserStatus.Active;
        }

        if (!user.UserAddresses.Any())
        {
            user.UserAddresses.Add(new UserAddress
            {
                UserId = user.Id,
                ReceiverName = fullName,
                Phone = "0900000000",
                AddressLine = "123 Phase 2 Street",
                City = "Ho Chi Minh City",
                District = "District 1",
                Ward = "Ben Nghe",
                Label = "Default",
                IsDefault = true
            });
        }

        if (role == UserRole.Seller)
        {
            user.SellerProfile ??= new SellerProfile { UserId = user.Id };
            user.SellerProfile.ShopName = shopName ?? fullName;
            user.SellerProfile.Status = SellerStatus.Active;
            user.SellerProfile.CommissionRate = 0.05m;
            user.SellerProfile.ContactEmail = email;
        }

        await db.SaveChangesAsync(ct);
        return user;
    }

    private static async Task<ProductVariant> EnsureProductVariantAsync(
        ShopDbContext db,
        Guid sellerId,
        Guid categoryId,
        string slug,
        string productName,
        string sku,
        decimal price,
        CancellationToken ct)
    {
        var product = await db.Products
            .Include(p => p.Variants)
            .FirstOrDefaultAsync(p => p.Slug == slug, ct);

        if (product is null)
        {
            product = new Product
            {
                Name = productName,
                Slug = slug,
                Description = $"{productName} for phase 2 verification.",
                BasePrice = price,
                CategoryId = categoryId,
                IsActive = true,
                SellerId = sellerId,
                ApprovalStatus = ApprovalStatus.Approved
            };
            db.Products.Add(product);
            await db.SaveChangesAsync(ct);
        }
        else
        {
            product.Name = productName;
            product.BasePrice = price;
            product.CategoryId = categoryId;
            product.SellerId = sellerId;
            product.ApprovalStatus = ApprovalStatus.Approved;
            product.IsActive = true;
        }

        var variant = product.Variants.FirstOrDefault(v => v.Sku == sku);
        if (variant is null)
        {
            variant = new ProductVariant
            {
                ProductId = product.Id,
                Sku = sku,
                Price = price,
                StockQuantity = 20,
                IsActive = true
            };
            db.ProductVariants.Add(variant);
            await db.SaveChangesAsync(ct);
        }
        else
        {
            variant.Price = price;
            variant.StockQuantity = 20;
            variant.IsActive = true;
        }

        variant.Product = product;
        await db.SaveChangesAsync(ct);
        return variant;
    }

    private static async Task<Order> EnsureOrderAsync(
        ShopDbContext db,
        string note,
        User customer,
        UserAddress address,
        User seller,
        ProductVariant variant,
        OrderStatus status,
        decimal shippingFee,
        int quantity,
        DateTime createdAt,
        DateTime changedAt,
        CancellationToken ct)
    {
        var order = await db.Orders
            .Include(o => o.OrderItems)
            .Include(o => o.OrderStatusHistory)
            .AsSplitQuery()
            .FirstOrDefaultAsync(o => o.Note == note, ct);

        var subtotal = variant.Price * quantity;

        if (order is null)
        {
            order = new Order
            {
                UserId = customer.Id,
                ShippingReceiverName = address.ReceiverName,
                ShippingPhone = address.Phone,
                ShippingAddressLine = address.AddressLine,
                ShippingCity = address.City,
                ShippingDistrict = address.District,
                ShippingWard = address.Ward,
                SubtotalAmount = subtotal,
                DiscountAmount = 0,
                ShippingFee = shippingFee,
                TotalAmount = subtotal + shippingFee,
                Currency = "VND",
                Status = status,
                Note = note,
                CreatedAt = createdAt,
                UpdatedAt = changedAt
            };
            db.Orders.Add(order);
            await db.SaveChangesAsync(ct);
        }
        else
        {
            order.UserId = customer.Id;
            order.SubtotalAmount = subtotal;
            order.ShippingFee = shippingFee;
            order.TotalAmount = subtotal + shippingFee;
            order.Status = status;
            order.CreatedAt = createdAt;
            order.UpdatedAt = changedAt;
        }

        var item = order.OrderItems.FirstOrDefault();
        if (item is null)
        {
            item = new OrderItem { OrderId = order.Id };
            order.OrderItems.Add(item);
        }

        item.ProductVariantId = variant.Id;
        item.SkuSnapshot = variant.Sku;
        item.ProductNameSnapshot = variant.Product!.Name;
        item.Quantity = quantity;
        item.UnitPrice = variant.Price;
        item.TotalPrice = subtotal;
        item.SellerId = seller.Id;
        item.CreatedAt = createdAt;
        item.UpdatedAt = changedAt;

        foreach (var extraItem in order.OrderItems.Skip(1).ToList())
        {
            db.OrderItems.Remove(extraItem);
        }

        var history = order.OrderStatusHistory.FirstOrDefault();
        if (history is null)
        {
            history = new OrderStatusHistory { OrderId = order.Id };
            order.OrderStatusHistory.Add(history);
        }

        history.Status = status;
        history.Note = $"Seeded {note}";
        history.ChangedBy = seller.Id;
        history.ChangedAt = changedAt;
        history.CreatedAt = createdAt;
        history.UpdatedAt = changedAt;

        foreach (var extraHistory in order.OrderStatusHistory.Skip(1).ToList())
        {
            db.OrderStatusHistory.Remove(extraHistory);
        }

        await db.SaveChangesAsync(ct);
        return order;
    }
}
