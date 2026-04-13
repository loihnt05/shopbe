using Shopbe.Domain.Enums;

namespace Shopbe.Application.Payment.Refund.Dtos;

public sealed record RefundDto(
    Guid Id,
    Guid PaymentId,
    decimal Amount,
    string? Reason,
    RefundStatus Status,
    Guid? ProcessedBy,
    DateTime CreatedAt);