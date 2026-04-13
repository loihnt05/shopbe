using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shopbe.Application.Cart.Dtos;
using Shopbe.Application.Order.Dtos;
using Shopbe.E2E.Tests.Infrastructure;
using Shopbe.Infrastructure.Persistence;

namespace Shopbe.E2E.Tests;

public sealed class PurchaseFlowTests : IClassFixture<PostgresFixture>
{
    private readonly PostgresFixture _postgres;

    public PurchaseFlowTests(PostgresFixture postgres)
    {
        _postgres = postgres;
    }

    private sealed class CreateStripePaymentIntentResponse
    {
        public Guid PaymentId { get; set; }
        public string ClientSecret { get; set; } = string.Empty;
        public string PaymentIntentId { get; set; } = string.Empty;
    }

    [Fact]
    public async Task Customer_can_buy_and_pay_for_a_product_happy_path()
    {
        // Arrange
        await using var factory = new ShopbeApiFactory(_postgres.ConnectionString);

        // Seed: category + product + variant
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ShopDbContext>();
            await DatabaseSeed.SeedMinimalCatalogAsync(db);
        }

        var client = factory.CreateAuthenticatedClient(keycloakSub: "e2e-sub-1", email: "e2e@local.test");

        // Get seeded variant id
        Guid variantId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ShopDbContext>();
            variantId = db.ProductVariants.Select(v => v.Id).First();
        }

        // Act 1: add to cart
        var addCartResp = await client.PostAsJsonAsync("/api/cart/items", new AddCartItemRequestDto(variantId, 1));
        addCartResp.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act 2: create order
        var orderRequest = new CreateOrderRequestDto
        {
            UseDefaultAddressIfAvailable = false,
            ShippingReceiverName = "E2E Receiver",
            ShippingPhone = "0900000000",
            ShippingAddressLine = "1 Test Street",
            ShippingCity = "HCM",
            ShippingDistrict = "D1",
            ShippingWard = "W1",
            Note = "E2E order"
        };

        var createOrderResp = await client.PostAsJsonAsync("/api/orders", orderRequest);
        createOrderResp.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdOrderJson = await createOrderResp.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        createdOrderJson.TryGetProperty("id", out var orderIdEl).Should().BeTrue("create order response must return id");
        var orderId = orderIdEl.GetGuid();

        // Act 3: create stripe payment intent (fake stripe)
        var intentResp = await client.PostAsJsonAsync("/api/payments/stripe/payment-intents", new { orderId });
        intentResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var intent = await intentResp.Content.ReadFromJsonAsync<CreateStripePaymentIntentResponse>();
        intent.Should().NotBeNull();
        intent!.PaymentIntentId.Should().StartWith("pi_test_");
        intent.ClientSecret.Should().StartWith("cs_test_");

        // Act 4: simulate Stripe webhook "payment_intent.succeeded"
        var stripeEventPayload = new
        {
            id = "evt_test_1",
            type = "payment_intent.succeeded",
            data = new
            {
                @object = new
                {
                    id = intent.PaymentIntentId
                }
            }
        };

        // Signature header is required by controller; fake stripe ignores it.
        var webhookReq = new HttpRequestMessage(HttpMethod.Post, "/api/payments/stripe/webhook")
        {
            Content = JsonContent.Create(stripeEventPayload)
        };
        webhookReq.Headers.Add("Stripe-Signature", "test");

        var webhookResp = await client.SendAsync(webhookReq);
        webhookResp.StatusCode.Should().Be(HttpStatusCode.OK);

        // Assert: order becomes Confirmed and payment becomes Paid.
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ShopDbContext>();

            var order = await db.Orders.Include(o => o.Payments).FirstAsync(o => o.Id == orderId);
            order.Status.Should().Be(Shopbe.Domain.Enums.OrderStatus.Confirmed);

            var payment = order.Payments.OrderByDescending(p => p.CreatedAt).First();
            payment.Status.Should().Be(Shopbe.Domain.Enums.PaymentStatus.Paid);
            payment.PaidAt.Should().NotBeNull();
        }
    }
}


