using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shopbe.Domain.Entities.Category;
using Shopbe.Domain.Entities.Order;
using Shopbe.Domain.Entities.Product;
using Shopbe.Domain.Entities.Seller;
using Shopbe.Domain.Entities.Shipping;
using Shopbe.Domain.Entities.User;
using Shopbe.Domain.Enums;

namespace Shopbe.Infrastructure.Persistence;

/// <summary>
/// Simple development seeder to create a few categories/products/variants so you can test the buying flow manually.
/// Safe to run multiple times (idempotent by slug/sku).
/// </summary>
public static class ShopbeDbSeeder
{
    private sealed record DemoUserSeed(
        string PlaceholderKeycloakId,
        string Email,
        string FullName,
        UserRole Role,
        string PhoneNumber,
        string City,
        string District,
        string Ward,
        string AddressLine,
        string? ShopName = null,
        string? ShopDescription = null);

    private static readonly DemoUserSeed[] DemoUsers =
    [
        new(
            PlaceholderKeycloakId: "demo-admin-keycloak-id",
            Email: "admin@shopbee.vn",
            FullName: "System Admin",
            Role: UserRole.Admin,
            PhoneNumber: "0900000001",
            City: "Ho Chi Minh City",
            District: "District 1",
            Ward: "Ben Nghe",
            AddressLine: "1 Admin Street"),
        new(
            PlaceholderKeycloakId: "demo-seller1-keycloak-id",
            Email: "seller1@shopbee.vn",
            FullName: "Seller One",
            Role: UserRole.Seller,
            PhoneNumber: "0900000002",
            City: "Ho Chi Minh City",
            District: "District 3",
            Ward: "Ward 7",
            AddressLine: "12 Seller Street",
            ShopName: "Luna Tech",
            ShopDescription: "Electronics and desk setup accessories."),
        new(
            PlaceholderKeycloakId: "demo-seller2-keycloak-id",
            Email: "seller2@shopbee.vn",
            FullName: "Seller Two",
            Role: UserRole.Seller,
            PhoneNumber: "0900000003",
            City: "Ha Noi",
            District: "Cau Giay",
            Ward: "Dich Vong",
            AddressLine: "88 Market Avenue",
            ShopName: "Northwind Home",
            ShopDescription: "Home office and lifestyle goods."),
        new(
            PlaceholderKeycloakId: "demo-customer1-keycloak-id",
            Email: "customer1@shopbee.vn",
            FullName: "Customer One",
            Role: UserRole.Customer,
            PhoneNumber: "0900000004",
            City: "Da Nang",
            District: "Hai Chau",
            Ward: "Thach Thang",
            AddressLine: "25 Customer Lane")
    ];

    public static async Task SeedAsync(ShopDbContext db, ILogger? logger = null, CancellationToken ct = default)
    {
        await EnsureDemoUsersAsync(db, logger, ct);
        await ShippingDataSeeder.SeedAsync(db, logger, ct);
        await SeedCouponsAsync(db, logger, ct);
        await SeedAttributesAsync(db, logger, ct);

        logger?.LogInformation("Seeding sample catalog data...");

        await DummyJsonSeeder.SeedAsync(db, logger, ct: ct);
        await EscuelaSeeder.SeedAsync(db, logger, ct: ct);
        await SeedTestProductsAsync(db, logger, ct);
        await SeedGamingLaptopAsync(db, logger, ct);

        await db.SaveChangesAsync(ct);
        
        await BackfillProductSellerIdsAsync(db, logger, ct);
        await SeedSellerShowcaseProductsAsync(db, logger, ct);
        await SeedDemoOrdersAsync(db, logger, ct);
        await AssignRandomAttributesToVariantsAsync(db, logger, ct);

        logger?.LogInformation("Seeded sample catalog: {Count} products.", await db.Products.CountAsync(ct));
    }

    private static async Task EnsureDemoUsersAsync(ShopDbContext db, ILogger? logger, CancellationToken ct)
    {
        foreach (var demoUser in DemoUsers)
        {
            var user = await db.Users
                .Include(u => u.SellerProfile)
                .Include(u => u.UserAddresses)
                .Include(u => u.ShoppingCart)
                .FirstOrDefaultAsync(u => u.Email == demoUser.Email, ct);

            var created = false;
            if (user is null)
            {
                user = new User
                {
                    KeycloakId = demoUser.PlaceholderKeycloakId,
                    Email = demoUser.Email,
                    FullName = demoUser.FullName,
                    Role = demoUser.Role,
                    Status = UserStatus.Active,
                    PhoneNumber = demoUser.PhoneNumber,
                    Country = "Vietnam",
                    Language = "vi-VN"
                };
                db.Users.Add(user);
                await db.SaveChangesAsync(ct);
                created = true;
            }
            else
            {
                user.FullName = demoUser.FullName;
                user.Role = demoUser.Role;
                user.Status = UserStatus.Active;
                user.PhoneNumber = demoUser.PhoneNumber;
                user.Country = user.Country ?? "Vietnam";
                user.Language = user.Language ?? "vi-VN";
                if (string.IsNullOrWhiteSpace(user.KeycloakId))
                {
                    user.KeycloakId = demoUser.PlaceholderKeycloakId;
                }
            }

            if (!user.UserAddresses.Any())
            {
                user.UserAddresses.Add(new UserAddress
                {
                    UserId = user.Id,
                    ReceiverName = user.FullName,
                    Phone = demoUser.PhoneNumber,
                    AddressLine = demoUser.AddressLine,
                    City = demoUser.City,
                    District = demoUser.District,
                    Ward = demoUser.Ward,
                    Label = "Default",
                    IsDefault = true
                });
            }

            if (user.ShoppingCart is null)
            {
                user.ShoppingCart = new Shopbe.Domain.Entities.ShoppingCart.ShoppingCart
                {
                    UserId = user.Id
                };
            }

            if (demoUser.Role == UserRole.Seller)
            {
                user.SellerProfile ??= new SellerProfile
                {
                    UserId = user.Id
                };

                user.SellerProfile.ShopName = demoUser.ShopName ?? user.FullName;
                user.SellerProfile.ShopDescription = demoUser.ShopDescription;
                user.SellerProfile.ContactEmail = user.Email;
                user.SellerProfile.ContactPhone = demoUser.PhoneNumber;
                user.SellerProfile.Address = demoUser.AddressLine;
                user.SellerProfile.City = demoUser.City;
                user.SellerProfile.Status = SellerStatus.Active;
                user.SellerProfile.CommissionRate = 0.05m;
            }

            await db.SaveChangesAsync(ct);
            logger?.LogInformation("Ensured demo user: {Email} ({Role}){Suffix}", user.Email, user.Role, created ? " [created]" : string.Empty);
        }
    }

    private static async Task BackfillProductSellerIdsAsync(ShopDbContext db, ILogger? logger, CancellationToken ct)
    {
        var adminUser = await db.Users.FirstOrDefaultAsync(u => u.Role == UserRole.Admin, ct);
        if (adminUser == null) return;

        var productsWithoutSeller = await db.Products
            .Where(p => p.SellerId == Guid.Empty)
            .ToListAsync(ct);

        if (productsWithoutSeller.Count == 0) return;

        foreach (var product in productsWithoutSeller)
        {
            product.SellerId = adminUser.Id;
        }

        await db.SaveChangesAsync(ct);
        logger?.LogInformation("Backfilled {Count} products with admin SellerId.", productsWithoutSeller.Count);
    }

    private static async Task SeedSellerShowcaseProductsAsync(ShopDbContext db, ILogger? logger, CancellationToken ct)
    {
        var seller1 = await db.Users.FirstOrDefaultAsync(u => u.Email == "seller1@shopbee.vn", ct);
        var seller2 = await db.Users.FirstOrDefaultAsync(u => u.Email == "seller2@shopbee.vn", ct);
        if (seller1 is null || seller2 is null)
        {
            return;
        }

        var category = await db.Categories.FirstOrDefaultAsync(c => c.Slug == "electronics", ct)
            ?? await db.Categories.FirstOrDefaultAsync(c => c.IsActive, ct);

        if (category is null)
        {
            category = new Category
            {
                Name = "Electronics",
                Slug = "electronics",
                IsActive = true
            };
            db.Categories.Add(category);
            await db.SaveChangesAsync(ct);
        }

        await EnsureDemoProductAsync(
            db,
            seller1.Id,
            category.Id,
            slug: "luna-wireless-headphones",
            name: "Luna Wireless Headphones",
            description: "Seller demo product for testing the approved seller catalog flow.",
            basePrice: 1290000m,
            approvalStatus: ApprovalStatus.Approved,
            imageUrl: "https://picsum.photos/seed/luna-wireless-headphones/800/800",
            sku: "LUNA-HEADPHONE-01",
            stockQuantity: 24,
            logger: logger,
            ct: ct);

        await EnsureDemoProductAsync(
            db,
            seller2.Id,
            category.Id,
            slug: "northwind-monitor-stand",
            name: "Northwind Monitor Stand",
            description: "Seller demo product for testing another active seller account.",
            basePrice: 690000m,
            approvalStatus: ApprovalStatus.Approved,
            imageUrl: "https://picsum.photos/seed/northwind-monitor-stand/800/800",
            sku: "NORTHWIND-STAND-01",
            stockQuantity: 18,
            logger: logger,
            ct: ct);

        await EnsureDemoProductAsync(
            db,
            seller2.Id,
            category.Id,
            slug: "northwind-desk-lamp",
            name: "Northwind Desk Lamp",
            description: "Seller demo product left pending so admin moderation can be tested quickly.",
            basePrice: 450000m,
            approvalStatus: ApprovalStatus.Pending,
            imageUrl: "https://picsum.photos/seed/northwind-desk-lamp/800/800",
            sku: "NORTHWIND-LAMP-01",
            stockQuantity: 10,
            logger: logger,
            ct: ct);
    }

    private static async Task SeedDemoOrdersAsync(ShopDbContext db, ILogger? logger, CancellationToken ct)
    {
        var customer = await db.Users
            .Include(u => u.UserAddresses)
            .FirstOrDefaultAsync(u => u.Email == "customer1@shopbee.vn", ct);
        var seller1 = await db.Users
            .Include(u => u.SellerProfile)
            .FirstOrDefaultAsync(u => u.Email == "seller1@shopbee.vn", ct);
        var seller2 = await db.Users
            .Include(u => u.SellerProfile)
            .FirstOrDefaultAsync(u => u.Email == "seller2@shopbee.vn", ct);

        if (customer is null || seller1 is null || seller2 is null)
        {
            return;
        }

        var defaultAddress = customer.UserAddresses.FirstOrDefault(a => a.IsDefault) ?? customer.UserAddresses.FirstOrDefault();
        if (defaultAddress is null)
        {
            return;
        }

        var seller1Variant = await db.ProductVariants
            .Include(v => v.Product)
            .FirstOrDefaultAsync(v => v.Sku == "LUNA-HEADPHONE-01", ct);
        var seller2Variant = await db.ProductVariants
            .Include(v => v.Product)
            .FirstOrDefaultAsync(v => v.Sku == "NORTHWIND-STAND-01", ct);

        if (seller1Variant?.Product is null || seller2Variant?.Product is null)
        {
            return;
        }

        await EnsureDemoOrderAsync(
            db,
            customer,
            defaultAddress,
            seller1,
            seller1Variant,
            status: OrderStatus.Delivered,
            shippingFee: 30000m,
            quantity: 1,
            note: "demo-order-seller1-delivered",
            createdAt: DateTime.UtcNow.AddDays(-7),
            changedAt: DateTime.UtcNow.AddDays(-5),
            logger: logger,
            ct: ct);

        await EnsureDemoOrderAsync(
            db,
            customer,
            defaultAddress,
            seller2,
            seller2Variant,
            status: OrderStatus.Pending,
            shippingFee: 25000m,
            quantity: 1,
            note: "demo-order-seller2-pending",
            createdAt: DateTime.UtcNow.AddDays(-2),
            changedAt: DateTime.UtcNow.AddDays(-2),
            logger: logger,
            ct: ct);

        seller1.SellerProfile!.TotalSales = await db.OrderItems
            .Where(i => i.SellerId == seller1.Id)
            .SumAsync(i => i.Quantity, ct);
        seller1.SellerProfile.TotalRevenue = await db.Orders
            .Where(o => o.Status == OrderStatus.Confirmed || o.Status == OrderStatus.Processing || o.Status == OrderStatus.Shipped || o.Status == OrderStatus.Delivered)
            .SelectMany(o => o.OrderItems)
            .Where(i => i.SellerId == seller1.Id)
            .SumAsync(i => i.TotalPrice, ct);

        seller2.SellerProfile!.TotalSales = await db.OrderItems
            .Where(i => i.SellerId == seller2.Id)
            .SumAsync(i => i.Quantity, ct);
        seller2.SellerProfile.TotalRevenue = await db.Orders
            .Where(o => o.Status == OrderStatus.Confirmed || o.Status == OrderStatus.Processing || o.Status == OrderStatus.Shipped || o.Status == OrderStatus.Delivered)
            .SelectMany(o => o.OrderItems)
            .Where(i => i.SellerId == seller2.Id)
            .SumAsync(i => i.TotalPrice, ct);

        await db.SaveChangesAsync(ct);
    }

    private static async Task EnsureDemoOrderAsync(
        ShopDbContext db,
        User customer,
        UserAddress shippingAddress,
        User seller,
        ProductVariant variant,
        OrderStatus status,
        decimal shippingFee,
        int quantity,
        string note,
        DateTime createdAt,
        DateTime changedAt,
        ILogger? logger,
        CancellationToken ct)
    {
        var order = await db.Orders
            .Include(o => o.OrderItems)
            .Include(o => o.OrderStatusHistory)
            .FirstOrDefaultAsync(o => o.Note == note, ct);

        var subtotal = variant.Price * quantity;
        var total = subtotal + shippingFee;

        if (order is null)
        {
            order = new Order
            {
                UserId = customer.Id,
                ShippingReceiverName = shippingAddress.ReceiverName,
                ShippingPhone = shippingAddress.Phone,
                ShippingAddressLine = shippingAddress.AddressLine,
                ShippingCity = shippingAddress.City,
                ShippingDistrict = shippingAddress.District,
                ShippingWard = shippingAddress.Ward,
                SubtotalAmount = subtotal,
                DiscountAmount = 0,
                ShippingFee = shippingFee,
                TotalAmount = total,
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
            order.ShippingReceiverName = shippingAddress.ReceiverName;
            order.ShippingPhone = shippingAddress.Phone;
            order.ShippingAddressLine = shippingAddress.AddressLine;
            order.ShippingCity = shippingAddress.City;
            order.ShippingDistrict = shippingAddress.District;
            order.ShippingWard = shippingAddress.Ward;
            order.SubtotalAmount = subtotal;
            order.DiscountAmount = 0;
            order.ShippingFee = shippingFee;
            order.TotalAmount = total;
            order.Currency = "VND";
            order.Status = status;
            order.Note = note;
            order.CreatedAt = createdAt;
            order.UpdatedAt = changedAt;
        }

        var orderItem = order.OrderItems.FirstOrDefault();
        if (orderItem is null)
        {
            orderItem = new OrderItem
            {
                OrderId = order.Id
            };
            order.OrderItems.Add(orderItem);
        }

        orderItem.ProductVariantId = variant.Id;
        orderItem.SkuSnapshot = variant.Sku;
        orderItem.ProductNameSnapshot = variant.Product!.Name;
        orderItem.Quantity = quantity;
        orderItem.UnitPrice = variant.Price;
        orderItem.TotalPrice = subtotal;
        orderItem.SellerId = seller.Id;
        orderItem.CreatedAt = createdAt;
        orderItem.UpdatedAt = changedAt;

        foreach (var extraItem in order.OrderItems.Skip(1).ToList())
        {
            db.OrderItems.Remove(extraItem);
        }

        var statusHistory = order.OrderStatusHistory.FirstOrDefault();
        if (statusHistory is null)
        {
            statusHistory = new OrderStatusHistory
            {
                OrderId = order.Id
            };
            order.OrderStatusHistory.Add(statusHistory);
        }

        statusHistory.Status = status;
        statusHistory.Note = $"Seeded status for {note}";
        statusHistory.ChangedBy = seller.Id;
        statusHistory.ChangedAt = changedAt;
        statusHistory.CreatedAt = createdAt;
        statusHistory.UpdatedAt = changedAt;

        foreach (var extraHistory in order.OrderStatusHistory.Skip(1).ToList())
        {
            db.OrderStatusHistory.Remove(extraHistory);
        }

        variant.Product.SoldCount = await db.OrderItems
            .Where(i => i.ProductVariant!.ProductId == variant.ProductId)
            .SumAsync(i => i.Quantity, ct);

        await db.SaveChangesAsync(ct);
        logger?.LogInformation("Ensured demo order: {Note} ({Status})", note, status);
    }

    private static async Task EnsureDemoProductAsync(
        ShopDbContext db,
        Guid sellerId,
        Guid categoryId,
        string slug,
        string name,
        string description,
        decimal basePrice,
        ApprovalStatus approvalStatus,
        string imageUrl,
        string sku,
        int stockQuantity,
        ILogger? logger,
        CancellationToken ct)
    {
        var product = await db.Products
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .FirstOrDefaultAsync(p => p.Slug == slug, ct);

        if (product is null)
        {
            product = new Product
            {
                Name = name,
                Slug = slug,
                Description = description,
                BasePrice = basePrice,
                CategoryId = categoryId,
                SellerId = sellerId,
                ApprovalStatus = approvalStatus,
                IsActive = true,
                SoldCount = 0
            };
            db.Products.Add(product);
            await db.SaveChangesAsync(ct);
            logger?.LogInformation("Seeded demo seller product: {Slug}", slug);
        }
        else
        {
            product.Name = name;
            product.Description = description;
            product.BasePrice = basePrice;
            product.CategoryId = categoryId;
            product.SellerId = sellerId;
            product.ApprovalStatus = approvalStatus;
            product.IsActive = true;
            await db.SaveChangesAsync(ct);
        }

        if (!product.Images.Any())
        {
            db.ProductImages.Add(new ProductImage
            {
                ProductId = product.Id,
                ImageUrl = imageUrl,
                AltText = name,
                IsPrimary = true,
                SortOrder = 0
            });
        }

        if (!product.Variants.Any())
        {
            db.ProductVariants.Add(new ProductVariant
            {
                ProductId = product.Id,
                Sku = sku,
                Price = basePrice,
                StockQuantity = stockQuantity,
                IsActive = true
            });
        }

        await db.SaveChangesAsync(ct);
    }

    private static async Task SeedGamingLaptopAsync(ShopDbContext db, ILogger? logger, CancellationToken ct)
    {
        var category = await db.Categories.FirstOrDefaultAsync(c => c.Name == "Electronics", ct);
        if (category == null)
        {
            category = new Category { Name = "Electronics", Slug = "electronics", IsActive = true };
            db.Categories.Add(category);
            await db.SaveChangesAsync(ct);
        }

        var productName = "Predator Helios Gaming Laptop";
        var productSlug = "predator-helios-gaming-laptop";
        
        var existingProduct = await db.Products.FirstOrDefaultAsync(p => p.Slug == productSlug, ct);
        if (existingProduct != null) return;

        var product = new Product
        {
            Name = productName,
            Slug = productSlug,
            Description = "Ultimate gaming performance with top-tier components. Customizable RAM, Storage, and GPU options to fit your needs.",
            BasePrice = 35000000,
            SoldCount = 45,
            CategoryId = category.Id,
            IsActive = true
        };
        db.Products.Add(product);
        await db.SaveChangesAsync(ct);

        db.ProductImages.Add(new ProductImage
        {
            ProductId = product.Id,
            ImageUrl = "https://images.unsplash.com/photo-1603302576837-37561b2e2302?ixlib=rb-4.0.3&auto=format&fit=crop&w=800&q=80",
            IsPrimary = true,
            AltText = productName
        });

        // Attributes
        var colors = new[] { "Obsidian Black", "Steel Gray" };
        var rams = new[] { "16GB", "32GB" };
        var storages = new[] { "512GB", "1TB", "2TB" };
        var gpus = new[] { "RTX 4070", "RTX 4080" };

        foreach (var color in colors)
        {
            foreach (var ram in rams)
            {
                foreach (var storage in storages)
                {
                    foreach (var gpu in gpus)
                    {
                        var sku = $"LAPTOP-{color.Substring(0, 3).ToUpper()}-{ram}-{storage}-{gpu.Replace(" ", "")}";
                        // Price increases with components
                        decimal price = 35000000;
                        if (ram == "32GB") price += 3000000;
                        if (storage == "1TB") price += 2000000;
                        else if (storage == "2TB") price += 4500000;
                        if (gpu == "RTX 4080") price += 8000000;

                        var variant = new ProductVariant
                        {
                            ProductId = product.Id,
                            Sku = sku,
                            Price = price,
                            StockQuantity = 10,
                            IsActive = true
                        };
                        db.ProductVariants.Add(variant);
                        await db.SaveChangesAsync(ct);

                        // Map attributes
                        var attrValues = new List<(string Name, string Value)>
                        {
                            ("Color", color),
                            ("RAM", ram),
                            ("Storage", storage),
                            ("GPU", gpu)
                        };

                        foreach (var av in attrValues)
                        {
                            var val = await db.AttributeValues.FirstOrDefaultAsync(v => v.Attribute!.Name == av.Name && v.Value == av.Value, ct);
                            if (val != null)
                            {
                                db.ProductVariantAttributes.Add(new ProductVariantAttribute { VariantId = variant.Id, AttributeValueId = val.Id });
                            }
                        }
                    }
                }
            }
        }

        logger?.LogInformation("Seeded multi-attribute gaming laptop: {Name}", productName);
    }

    private static async Task SeedAttributesAsync(ShopDbContext db, ILogger? logger, CancellationToken ct)
    {
        var attributes = new Dictionary<string, string[]>
        {
            { "Color", new[] { "Red", "Blue", "Green", "Black", "White", "Silver", "Gold", "Pink", "Purple", "Yellow", "Orange", "Grey", "Brown", "Obsidian Black", "Steel Gray" } },
            { "Size", new[] { "S", "M", "L", "XL", "XXL", "36", "37", "38", "39", "40", "41", "42", "43", "44", "45" } },
            { "Material", new[] { "Cotton", "Silk", "Leather", "Denim", "Polyester", "Nylon", "Wool", "Plastic", "Metal", "Wood", "Glass" } },
            { "Storage", new[] { "64GB", "128GB", "256GB", "512GB", "1TB", "2TB" } },
            { "RAM", new[] { "4GB", "8GB", "16GB", "32GB", "64GB" } },
            { "GPU", new[] { "RTX 4070", "RTX 4080", "RTX 4090" } }
        };

        var createdAttr = 0;
        var createdVals = 0;

        foreach (var attrPair in attributes)
        {
            var attrName = attrPair.Key;
            var values = attrPair.Value;

            var existingAttr = await db.ProductAttributes
                .Include(a => a.AttributeValues)
                .FirstOrDefaultAsync(a => a.Name == attrName, ct);

            if (existingAttr == null)
            {
                existingAttr = new ProductAttribute { Name = attrName };
                db.ProductAttributes.Add(existingAttr);
                createdAttr++;
            }

            foreach (var val in values)
            {
                if (existingAttr.AttributeValues.All(v => v.Value != val))
                {
                    db.AttributeValues.Add(new AttributeValue
                    {
                        AttributeId = existingAttr.Id,
                        Value = val
                    });
                    createdVals++;
                }
            }
        }

        if (createdAttr > 0 || createdVals > 0)
        {
            await db.SaveChangesAsync(ct);
            logger?.LogInformation("Seeded {AttrCount} attributes and {ValCount} attribute values.", createdAttr, createdVals);
        }
    }

    private static async Task AssignRandomAttributesToVariantsAsync(ShopDbContext db, ILogger? logger, CancellationToken ct)
    {
        // Only assign if we have variants and no assignments yet (to keep it idempotent-ish)
        if (await db.ProductVariantAttributes.AnyAsync(ct)) return;

        var variants = await db.ProductVariants.ToListAsync(ct);
        if (variants.Count == 0) return;

        var colors = await db.AttributeValues
            .Where(v => v.Attribute!.Name == "Color")
            .ToListAsync(ct);
        var sizes = await db.AttributeValues
            .Where(v => v.Attribute!.Name == "Size")
            .ToListAsync(ct);
        var materials = await db.AttributeValues
            .Where(v => v.Attribute!.Name == "Material")
            .ToListAsync(ct);

        var random = new Random();
        var assignments = 0;

        foreach (var variant in variants)
        {
            var possibleValues = new List<AttributeValue>();
            
            var roll = random.Next(100);
            if (roll < 40 && colors.Count > 0) 
            {
                possibleValues.Add(colors[random.Next(colors.Count)]);
            }
            
            roll = random.Next(100);
            if (roll < 30 && sizes.Count > 0) 
            {
                possibleValues.Add(sizes[random.Next(sizes.Count)]);
            }

            roll = random.Next(100);
            if (roll < 20 && materials.Count > 0) 
            {
                possibleValues.Add(materials[random.Next(materials.Count)]);
            }

            foreach (var val in possibleValues)
            {
                db.ProductVariantAttributes.Add(new ProductVariantAttribute
                {
                    VariantId = variant.Id,
                    AttributeValueId = val.Id
                });
                assignments++;
            }
        }

        if (assignments > 0)
        {
            await db.SaveChangesAsync(ct);
            logger?.LogInformation("Randomly assigned {Count} attributes to product variants.", assignments);
        }
    }

    private static async Task SeedCouponsAsync(ShopDbContext db, ILogger? logger, CancellationToken ct)
    {
        var desiredCoupons = new List<Coupon>
        {
            new()
            {
                Code = "HELLO2026",
                Description = "10% off for new year",
                DiscountType = DiscountType.Percentage,
                Value = 10,
                MinOrderAmount = 0,
                MaxDiscountAmount = 1000000,
                ExpiredAt = DateTime.UtcNow.AddYears(1),
                Count = 1000,
                IsActive = true
            },
            new()
            {
                Code = "FREESHIP",
                Description = "Free shipping for orders over 500k",
                DiscountType = DiscountType.FreeShipping,
                Value = 0,
                MinOrderAmount = 500000,
                ExpiredAt = DateTime.UtcNow.AddYears(1),
                Count = 999999,
                IsActive = true
            },
            new()
            {
                Code = "SAVE50K",
                Description = "50k off for orders over 200k",
                DiscountType = DiscountType.FixedAmount,
                Value = 50000,
                MinOrderAmount = 200000,
                ExpiredAt = DateTime.UtcNow.AddYears(1),
                Count = 999999,
                IsActive = true
            },
            new()
            {
                Code = "BIGSALE",
                Description = "50% off! (Max 200k discount)",
                DiscountType = DiscountType.Percentage,
                Value = 50,
                MinOrderAmount = 100000,
                MaxDiscountAmount = 200000,
                ExpiredAt = DateTime.UtcNow.AddYears(1),
                Count = 500,
                IsActive = true
            },
            new()
            {
                Code = "FLASH20",
                Description = "Limited time 20% off!",
                DiscountType = DiscountType.Percentage,
                Value = 20,
                MinOrderAmount = 0,
                ExpiredAt = DateTime.UtcNow.AddDays(7),
                Count = 10,
                IsActive = true
            },
            new()
            {
                Code = "WELCOME100",
                Description = "100k off for big spenders (Min 1M)",
                DiscountType = DiscountType.FixedAmount,
                Value = 100000,
                MinOrderAmount = 1000000,
                ExpiredAt = DateTime.UtcNow.AddYears(1),
                Count = 50,
                IsActive = true
            },
            new()
            {
                Code = "EXHAUSTED",
                Description = "This coupon is all gone",
                DiscountType = DiscountType.Percentage,
                Value = 99,
                MinOrderAmount = 0,
                ExpiredAt = DateTime.UtcNow.AddYears(1),
                Count = 0,
                IsActive = true
            },
            new()
            {
                Code = "EXPIRED",
                Description = "This coupon ended yesterday",
                DiscountType = DiscountType.Percentage,
                Value = 50,
                MinOrderAmount = 0,
                ExpiredAt = DateTime.UtcNow.AddDays(-1),
                Count = 100,
                IsActive = true
            }
        };

        var codes = desiredCoupons.Select(c => c.Code).ToArray();
        var existingCodes = await db.Coupons
            .Where(c => codes.Contains(c.Code))
            .Select(c => c.Code)
            .ToListAsync(ct);

        var created = 0;
        foreach (var coupon in desiredCoupons)
        {
            if (existingCodes.Contains(coupon.Code)) continue;

            db.Coupons.Add(coupon);
            created++;
        }

        if (created > 0)
        {
            await db.SaveChangesAsync(ct);
            logger?.LogInformation("Seeded {Count} sample coupons.", created);
        }
    }

    private static async Task SeedTestProductsAsync(ShopDbContext db, ILogger? logger, CancellationToken ct)
    {
        var category = await db.Categories.FirstOrDefaultAsync(c => c.Name == "Clothes", ct);
        if (category == null)
        {
            category = new Category { Name = "Clothes", Slug = "clothes", IsActive = true };
            db.Categories.Add(category);
            await db.SaveChangesAsync(ct);
        }

        var productName = "Premium Cotton T-Shirt";
        var productSlug = "premium-cotton-t-shirt";
        
        var existingProduct = await db.Products.FirstOrDefaultAsync(p => p.Slug == productSlug, ct);
        if (existingProduct != null) return;

        var product = new Product
        {
            Name = productName,
            Slug = productSlug,
            Description = "A high-quality 100% cotton t-shirt available in various colors and sizes. Comfortable, breathable, and perfect for any occasion.",
            BasePrice = 250000,
            SoldCount = 150,
            CategoryId = category.Id,
            IsActive = true
        };
        db.Products.Add(product);
        await db.SaveChangesAsync(ct);

        // Add an image
        db.ProductImages.Add(new ProductImage
        {
            ProductId = product.Id,
            ImageUrl = "https://images.unsplash.com/photo-1521572163474-6864f9cf17ab?ixlib=rb-4.0.3&auto=format&fit=crop&w=800&q=80",
            IsPrimary = true,
            AltText = productName
        });

        // Define variants
        var colors = new[] { "Red", "Blue", "Black" };
        var sizes = new[] { "S", "M", "L" };

        foreach (var color in colors)
        {
            foreach (var size in sizes)
            {
                var sku = $"TSHIRT-{color.ToUpper()}-{size}";
                var variant = new ProductVariant
                {
                    ProductId = product.Id,
                    Sku = sku,
                    Price = 250000,
                    StockQuantity = 50,
                    IsActive = true
                };
                db.ProductVariants.Add(variant);
                await db.SaveChangesAsync(ct);

                // Add attributes to this variant
                var colorAttr = await db.AttributeValues.FirstOrDefaultAsync(v => v.Attribute!.Name == "Color" && v.Value == color, ct);
                var sizeAttr = await db.AttributeValues.FirstOrDefaultAsync(v => v.Attribute!.Name == "Size" && v.Value == size, ct);

                if (colorAttr != null)
                {
                    db.ProductVariantAttributes.Add(new ProductVariantAttribute { VariantId = variant.Id, AttributeValueId = colorAttr.Id });
                }
                if (sizeAttr != null)
                {
                    db.ProductVariantAttributes.Add(new ProductVariantAttribute { VariantId = variant.Id, AttributeValueId = sizeAttr.Id });
                }
            }
        }

        logger?.LogInformation("Seeded multi-variant test product: {Name}", productName);
    }
}
