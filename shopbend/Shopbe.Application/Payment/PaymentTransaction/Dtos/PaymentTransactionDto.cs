using Shopbe.Domain.Enums;

namespace Shopbe.Application.Payment.PaymentTransaction.Dtos;

public sealed record PaymentTransactionDto(
    Guid Id,
    Guid PaymentId,
    string? GatewayTransactionId,
    decimal Amount,
    string Currency,
    PaymentStatus Status,
    DateTime CreatedAt);