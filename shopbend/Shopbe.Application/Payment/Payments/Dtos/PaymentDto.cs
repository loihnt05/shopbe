using Shopbe.Domain.Enums;

namespace Shopbe.Application.Payment.Payments.Dtos;

public sealed record PaymentDto(
    Guid Id,
    Guid OrderId,
    Shopbe.Domain.Enums.PaymentMethod Method,
    PaymentStatus Status,
    decimal Amount,
    string Currency,
    DateTime? PaidAt,
    string? StripePaymentIntentId,
    DateTime CreatedAt);