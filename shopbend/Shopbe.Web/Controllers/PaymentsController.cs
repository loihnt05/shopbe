using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.Payment.Payments.Dtos;
using Shopbe.Application.Payment.PaymentTransaction.Dtos;
using Shopbe.Application.Payment.Refund.Dtos;
using Shopbe.Domain.Entities.Payment;
using Shopbe.Domain.Enums;
using Shopbe.Infrastructure.Persistence;
using Stripe;

namespace Shopbe.Web.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentsController(
    IStripeService stripeService,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork,
    ShopDbContext dbContext) : ControllerBase
{
    private async Task<Guid> GetAppUserIdAsync(CancellationToken cancellationToken)
    {
        var keycloakId = currentUser.KeycloakId;
        if (string.IsNullOrWhiteSpace(keycloakId))
            throw new UnauthorizedAccessException("Missing user identity");

        var user = await unitOfWork.Users.GetUserByKeycloakIdAsync(keycloakId);
        if (user is null)
            throw new UnauthorizedAccessException("User not found");

        return user.Id;
    }

    /// <summary>
    /// Create a Stripe PaymentIntent for an existing order.
    /// Frontend uses returned clientSecret to confirm payment with Stripe.js.
    /// </summary>
    [HttpPost("stripe/payment-intents")]
    [Authorize]
    public async Task<ActionResult<CreateStripePaymentIntentResponse>> CreateStripePaymentIntent(
        [FromBody] CreateStripePaymentIntentRequest request,
        CancellationToken cancellationToken)
    {
        var userId = await GetAppUserIdAsync(cancellationToken);

        var order = await dbContext.Orders
            .Include(o => o.Payments)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId && o.UserId == userId, cancellationToken);

        if (order is null) return NotFound("Order not found");
        if (order.Status == OrderStatus.Cancelled) return BadRequest("Order was cancelled");
        if (order.Status == OrderStatus.Refunded) return BadRequest("Order is refunded");

        // Find existing pending Stripe payment for this order if any.
        var payment = order.Payments
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefault(p => p.Status == PaymentStatus.Pending && p.StripePaymentIntentId != null);

        if (payment is null)
        {
            payment = new Payment
            {
                OrderId = order.Id,
                Amount = order.TotalAmount,
                Currency = order.Currency,
                Status = PaymentStatus.Pending,
                Method = Shopbe.Domain.Enums.PaymentMethod.CreditCard // Stripe card payments
            };

            dbContext.Payments.Add(payment);
            await dbContext.SaveChangesAsync(cancellationToken);

            var (paymentIntentId, clientSecret) = await stripeService.CreatePaymentIntentAsync(
                order.Id,
                order.TotalAmount,
                order.Currency,
                cancellationToken);

            payment.StripePaymentIntentId = paymentIntentId;

            dbContext.PaymentTransactions.Add(new PaymentTransaction
            {
                PaymentId = payment.Id,
                Amount = payment.Amount,
                Currency = payment.Currency,
                Status = PaymentStatus.Pending,
                GatewayTransactionId = paymentIntentId,
                GatewayResponse = JsonDocument.Parse("{\"created\":true}")
            });

            await dbContext.SaveChangesAsync(cancellationToken);

            return Ok(new CreateStripePaymentIntentResponse
            {
                PaymentId = payment.Id,
                ClientSecret = clientSecret,
                PaymentIntentId = paymentIntentId
            });
        }

        // For existing payment, re-create intent if we don't have one (shouldn't happen)
        if (string.IsNullOrWhiteSpace(payment.StripePaymentIntentId))
        {
            var (paymentIntentId, clientSecret) = await stripeService.CreatePaymentIntentAsync(
                order.Id,
                payment.Amount,
                payment.Currency,
                cancellationToken);

            payment.StripePaymentIntentId = paymentIntentId;
            await dbContext.SaveChangesAsync(cancellationToken);

            return Ok(new CreateStripePaymentIntentResponse
            {
                PaymentId = payment.Id,
                ClientSecret = clientSecret,
                PaymentIntentId = paymentIntentId
            });
        }

        // If we already created an intent previously, Stripe does not allow retrieving client_secret without fetching intent.
        // To keep API simple, we create a fresh intent each time if client_secret is needed.
        // (Alternative: store clientSecret temporarily; not recommended to persist long-term.)
        var (newIntentId, newClientSecret) = await stripeService.CreatePaymentIntentAsync(
            order.Id,
            payment.Amount,
            payment.Currency,
            cancellationToken);

        payment.StripePaymentIntentId = newIntentId;
        payment.Status = PaymentStatus.Pending;
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new CreateStripePaymentIntentResponse
        {
            PaymentId = payment.Id,
            ClientSecret = newClientSecret,
            PaymentIntentId = newIntentId
        });
    }

    [HttpGet("{paymentId:guid}")]
    [Authorize]
    public async Task<ActionResult<PaymentDto>> GetPaymentById(Guid paymentId, CancellationToken cancellationToken)
    {
        var userId = await GetAppUserIdAsync(cancellationToken);
        var payment = await unitOfWork.Payments.GetByIdForUserAsync(paymentId, userId, cancellationToken);
        if (payment is null) return NotFound();

        return Ok(new PaymentDto(
            payment.Id,
            payment.OrderId,
            payment.Method,
            payment.Status,
            payment.Amount,
            payment.Currency,
            payment.PaidAt,
            payment.StripePaymentIntentId,
            payment.CreatedAt));
    }

    [HttpGet("by-order/{orderId:guid}")]
    [Authorize]
    public async Task<ActionResult<IReadOnlyList<PaymentDto>>> ListPaymentsByOrder(Guid orderId,
        CancellationToken cancellationToken)
    {
        var userId = await GetAppUserIdAsync(cancellationToken);
        var order = await unitOfWork.Orders.GetByIdForUserAsync(orderId, userId, cancellationToken);
        if (order is null) return NotFound("Order not found");

        var payments = await unitOfWork.Payments.ListByOrderIdAsync(orderId, cancellationToken);
        return Ok(payments.Select(p => new PaymentDto(
            p.Id,
            p.OrderId,
            p.Method,
            p.Status,
            p.Amount,
            p.Currency,
            p.PaidAt,
            p.StripePaymentIntentId,
            p.CreatedAt)).ToList());
    }

    [HttpGet("{paymentId:guid}/transactions")]
    [Authorize]
    public async Task<ActionResult<IReadOnlyList<PaymentTransactionDto>>> ListTransactions(Guid paymentId,
        CancellationToken cancellationToken)
    {
        var userId = await GetAppUserIdAsync(cancellationToken);
        var payment = await unitOfWork.Payments.GetByIdForUserAsync(paymentId, userId, cancellationToken);
        if (payment is null) return NotFound();

        var txs = await unitOfWork.PaymentTransactions.ListByPaymentIdAsync(paymentId, cancellationToken);
        return Ok(txs.Select(t => new PaymentTransactionDto(
            t.Id,
            t.PaymentId,
            t.GatewayTransactionId,
            t.Amount,
            t.Currency,
            t.Status,
            t.CreatedAt)).ToList());
    }

    public sealed class CreateRefundRequest
    {
        public decimal Amount { get; set; }
        public string? Reason { get; set; }
    }

    /// <summary>
    /// Create a Stripe refund for a payment. Admin-only.
    /// Idempotency supported via header: Idempotency-Key
    /// </summary>
    [HttpPost("{paymentId:guid}/refunds")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<RefundDto>> CreateRefund(Guid paymentId, [FromBody] CreateRefundRequest request,
        CancellationToken cancellationToken)
    {
        var idempotencyKey = Request.Headers.TryGetValue("Idempotency-Key", out var key)
            ? key.ToString()
            : null;

        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            var existing = await unitOfWork.IdempotencyKeys.GetAsync(idempotencyKey, IdempotencyEntityType.Refund,
                cancellationToken);
            if (existing?.Response is not null)
            {
                // Return cached response
                return Content(existing.Response.RootElement.GetRawText(), "application/json");
            }
        }

        var payment = await unitOfWork.Payments.GetByIdAsync(paymentId, cancellationToken);
        if (payment is null) return NotFound("Payment not found");
        if (payment.Status != PaymentStatus.Paid) return BadRequest("Payment is not paid");
        if (string.IsNullOrWhiteSpace(payment.StripePaymentIntentId))
            return BadRequest("Payment is missing StripePaymentIntentId");

        var amount = request.Amount <= 0 ? payment.Amount : request.Amount;
        if (amount > payment.Amount) return BadRequest("Refund amount exceeds payment amount");

        // Create refund at Stripe
        var stripeRefundId = await stripeService.CreateRefundAsync(payment.StripePaymentIntentId!, amount, cancellationToken);

        var refund = new Shopbe.Domain.Entities.Payment.Refund
        {
            PaymentId = payment.Id,
            Amount = amount,
            Reason = request.Reason,
            Status = RefundStatus.Completed,
            ProcessedBy = null
        };

        await unitOfWork.Refunds.AddAsync(refund, cancellationToken);

        // Update payment status (simple model: full refund => Refunded)
        if (amount == payment.Amount)
            payment.Status = PaymentStatus.Refunded;
        else
            payment.Status = PaymentStatus.PartiallyRefunded;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Log transaction line
        await unitOfWork.PaymentTransactions.AddAsync(new PaymentTransaction
        {
            PaymentId = payment.Id,
            Amount = amount,
            Currency = payment.Currency,
            Status = payment.Status,
            GatewayTransactionId = stripeRefundId,
            GatewayResponse = JsonDocument.Parse("{\"refund_id\":\"" + stripeRefundId + "\"}")
        }, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = new RefundDto(refund.Id, refund.PaymentId, refund.Amount, refund.Reason, refund.Status, refund.ProcessedBy,
            refund.CreatedAt);

        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            var json = JsonSerializer.SerializeToDocument(dto);
            await unitOfWork.IdempotencyKeys.UpsertResponseAsync(idempotencyKey!, IdempotencyEntityType.Refund, refund.Id, json,
                DateTime.UtcNow.AddHours(24), cancellationToken);
        }

        return Ok(dto);
    }

    /// <summary>
    /// Stripe webhook endpoint. Configure Stripe CLI / dashboard to send events here.
    /// </summary>
    [HttpPost("stripe/webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> StripeWebhook(CancellationToken cancellationToken)
    {
        // Must read raw body for signature verification
        using var reader = new StreamReader(Request.Body, Encoding.UTF8);
        var payload = await reader.ReadToEndAsync(cancellationToken);

        if (!Request.Headers.TryGetValue("Stripe-Signature", out var signatureHeader))
            return BadRequest("Missing Stripe-Signature header");

        Event stripeEvent;
        try
        {
            stripeEvent = stripeService.ConstructWebhookEvent(payload, signatureHeader!);
        }
        catch (Exception)
        {
            return BadRequest();
        }

        // Only handle the minimum required events
        switch (stripeEvent.Type)
        {
            case EventTypes.PaymentIntentSucceeded:
            {
                var intent = stripeEvent.Data.Object as PaymentIntent;
                if (intent is null) break;

                var paymentIntentId = intent.Id;
                var payment = await dbContext.Payments
                    .Include(p => p.Order)
                    .FirstOrDefaultAsync(p => p.StripePaymentIntentId == paymentIntentId, cancellationToken);

                if (payment is null) break;

                // Idempotency: ignore duplicates
                if (payment.LastStripeEventId == stripeEvent.Id)
                    return Ok();

                payment.LastStripeEventId = stripeEvent.Id;
                payment.Status = PaymentStatus.Paid;
                payment.PaidAt = DateTime.UtcNow;

                dbContext.PaymentTransactions.Add(new PaymentTransaction
                {
                    PaymentId = payment.Id,
                    Amount = payment.Amount,
                    Currency = payment.Currency,
                    Status = PaymentStatus.Paid,
                    GatewayTransactionId = stripeEvent.Id,
                    GatewayResponse = JsonDocument.Parse(payload)
                });

                if (payment.Order is not null && payment.Order.Status == OrderStatus.Pending)
                    payment.Order.Status = OrderStatus.Confirmed;

                await dbContext.SaveChangesAsync(cancellationToken);
                break;
            }

            case EventTypes.PaymentIntentPaymentFailed:
            {
                var intent = stripeEvent.Data.Object as PaymentIntent;
                if (intent is null) break;

                var payment = await dbContext.Payments
                    .Include(p => p.Order)
                    .FirstOrDefaultAsync(p => p.StripePaymentIntentId == intent.Id, cancellationToken);

                if (payment is null) break;

                if (payment.LastStripeEventId == stripeEvent.Id)
                    return Ok();

                payment.LastStripeEventId = stripeEvent.Id;
                payment.Status = PaymentStatus.Failed;

                dbContext.PaymentTransactions.Add(new PaymentTransaction
                {
                    PaymentId = payment.Id,
                    Amount = payment.Amount,
                    Currency = payment.Currency,
                    Status = PaymentStatus.Failed,
                    GatewayTransactionId = stripeEvent.Id,
                    GatewayResponse = JsonDocument.Parse(payload)
                });

                await dbContext.SaveChangesAsync(cancellationToken);
                break;
            }
        }

        return Ok();
    }
}




