using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Domain.Entities.Order;
using Shopbe.Domain.Enums;

namespace Shopbe.Web.Controllers;

[ApiController]
[Route("api/simulation")]
[Authorize]
public sealed class SimulationController(
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IBehaviorTrackingService tracking) : ControllerBase
{
    private async Task<Guid> GetAppUserIdAsync()
    {
        var keycloakId = currentUser.KeycloakId;
        if (string.IsNullOrWhiteSpace(keycloakId))
            throw new UnauthorizedAccessException("Missing user identity");

        var user = await unitOfWork.Users.GetUserByKeycloakIdAsync(keycloakId);
        if (user is null)
            throw new UnauthorizedAccessException("User not found");

        return user.Id;
    }

    [HttpPost("run")]
    public async Task<IActionResult> RunSimulation()
    {
        var userId = await GetAppUserIdAsync();
        
        // 1. Simulate Views across various categories to build a profile
        var categories = (await unitOfWork.Category.GetAllCategoriesAsync()).ToList();
        var products = (await unitOfWork.Product.GetAllProductsAsync()).ToList();
        
        if (!categories.Any() || !products.Any())
            return BadRequest("Database needs to be seeded first.");

        var random = new Random();
        
        // Pick 2 random categories to be "interested" in
        var interestedCategories = categories.OrderBy(x => Guid.NewGuid()).Take(2).ToList();
        
        foreach (var cat in interestedCategories)
        {
            var catProducts = products.Where(p => p.CategoryId == cat.Id).Take(5).ToList();
            foreach (var p in catProducts)
            {
                // Simulate 3-5 views for products in these categories
                int views = random.Next(3, 6);
                for (int i = 0; i < views; i++)
                {
                    await tracking.TrackAsync(
                        userId, null, null, 
                        BehaviorType.ProductView, "ProductView", 
                        p.Id, cat.Id, null, null, null, null, "Simulation");
                }
            }
        }

        // 2. Simulate "Frequently Bought Together" by creating mock orders
        // Pick a seed product and a complementary one
        var seedProduct = products.First();
        var complementaryProduct = products.Last();
        
        if (seedProduct.Id != complementaryProduct.Id)
        {
            for (int i = 0; i < 3; i++)
            {
                var order = new Order
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Status = OrderStatus.Confirmed,
                    TotalAmount = seedProduct.BasePrice + complementaryProduct.BasePrice,
                    CreatedAt = DateTime.UtcNow.AddDays(-i),
                    ShippingReceiverName = "Simulated User",
                    ShippingPhone = "0000000000",
                    ShippingAddressLine = "Simulation Address",
                    ShippingCity = "SimCity",
                    ShippingDistrict = "SimDistrict",
                    ShippingWard = "SimWard",
                    Currency = "VND"
                };
                
                var v1 = seedProduct.Variants.FirstOrDefault();
                var v2 = complementaryProduct.Variants.FirstOrDefault();

                if (v1 != null && v2 != null)
                {
                    if (seedProduct.SellerId == Guid.Empty || complementaryProduct.SellerId == Guid.Empty)
                    {
                        return BadRequest("Simulation products must have valid sellers before creating order items.");
                    }

                    order.OrderItems.Add(new OrderItem { 
                        Id = Guid.NewGuid(), 
                        ProductVariantId = v1.Id, 
                        Quantity = 1, 
                        UnitPrice = seedProduct.BasePrice, 
                        TotalPrice = seedProduct.BasePrice,
                        SellerId = seedProduct.SellerId,
                        SkuSnapshot = v1.Sku,
                        ProductNameSnapshot = seedProduct.Name
                    });
                    order.OrderItems.Add(new OrderItem { 
                        Id = Guid.NewGuid(), 
                        ProductVariantId = v2.Id, 
                        Quantity = 1, 
                        UnitPrice = complementaryProduct.BasePrice, 
                        TotalPrice = complementaryProduct.BasePrice,
                        SellerId = complementaryProduct.SellerId,
                        SkuSnapshot = v2.Sku,
                        ProductNameSnapshot = complementaryProduct.Name
                    });
                    
                    await unitOfWork.Orders.AddAsync(order);
                    
                    // Track purchase behavior
                    await tracking.TrackAsync(userId, null, null, BehaviorType.Purchase, "Purchase", seedProduct.Id, seedProduct.CategoryId, order.Id);
                    await tracking.TrackAsync(userId, null, null, BehaviorType.Purchase, "Purchase", complementaryProduct.Id, complementaryProduct.CategoryId, order.Id);
                }
            }
        }

        await unitOfWork.SaveChangesAsync();
        
        return Ok(new { 
            message = "Simulation completed.", 
            profileCategories = interestedCategories.Select(c => c.Name),
            seedProduct = seedProduct.Name,
            boughtWith = complementaryProduct.Name
        });
    }
}
