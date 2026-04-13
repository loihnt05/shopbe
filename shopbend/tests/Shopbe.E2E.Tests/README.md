# Shopbe.E2E.Tests

End-to-end API flow tests for the Shopbe backend.

## What is covered

`PurchaseFlowTests.Customer_can_buy_and_pay_for_a_product_happy_path` exercises the full customer journey:

1. Seed a minimal catalog (category + product + product variant)
2. Add product variant to cart (`POST /api/cart/items`)
3. Create an order from cart (`POST /api/orders`)
4. Create a Stripe PaymentIntent (`POST /api/payments/stripe/payment-intents`)
5. Simulate Stripe webhook `payment_intent.succeeded` (`POST /api/payments/stripe/webhook`)
6. Assert order is `Confirmed` and payment is `Paid`

## How it works (important)

- Uses **Testcontainers** to run an ephemeral **Postgres** (no local DB required).
- Uses an in-memory **TestServer** via `WebApplicationFactory<Program>`.
- Replaces Keycloak auth with a dedicated **test auth scheme** (`TestAuthHandler`) so endpoints annotated with `[Authorize]` work.
- Replaces Stripe with `FakeStripeService`, so the payment + webhook flow is deterministic.

## Run

```bash
cd shopbend

dotnet test tests/Shopbe.E2E.Tests/Shopbe.E2E.Tests.csproj -c Release
```

> Docker must be running because Testcontainers starts a Postgres container.

