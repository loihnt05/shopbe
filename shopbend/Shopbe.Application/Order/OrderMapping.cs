using Shopbe.Application.Order.Dtos;
using Shopbe.Domain.Entities.Order;

namespace Shopbe.Application.Order;

public static class OrderMapping
{
    public static OrderSummaryDto ToSummaryDto(this Domain.Entities.Order.Order order)
    {
        return new OrderSummaryDto
        {
            Id = order.Id,
            CreatedAt = order.CreatedAt,
            Status = order.Status,
            ItemsCount = order.OrderItems?.Sum(i => i.Quantity) ?? 0,
            TotalAmount = order.TotalAmount,
            Currency = order.Currency
        };
    }

    public static OrderDetailsDto ToDetailsDto(this Domain.Entities.Order.Order order)
    {
        return new OrderDetailsDto
        {
            Id = order.Id,
            UserId = order.UserId,
            CreatedAt = order.CreatedAt,
            Status = order.Status,
            ShippingReceiverName = order.ShippingReceiverName,
            ShippingPhone = order.ShippingPhone,
            ShippingAddressLine = order.ShippingAddressLine,
            ShippingCity = order.ShippingCity,
            ShippingDistrict = order.ShippingDistrict,
            ShippingWard = order.ShippingWard,
            Note = order.Note,
            SubtotalAmount = order.SubtotalAmount,
            DiscountAmount = order.DiscountAmount,
            ShippingFee = order.ShippingFee,
            TotalAmount = order.TotalAmount,
            Currency = order.Currency,
            Items = order.OrderItems
                .OrderBy(i => i.CreatedAt)
                .Select(i => new OrderItemDto
                {
                    ProductVariantId = i.ProductVariantId,
                    SkuSnapshot = i.SkuSnapshot,
                    ProductNameSnapshot = i.ProductNameSnapshot,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    TotalPrice = i.TotalPrice
                })
                .ToList(),
            History = order.OrderStatusHistory
                .OrderBy(h => h.ChangedAt)
                .Select(h => new OrderStatusHistoryDto
                {
                    Status = h.Status,
                    Note = h.Note,
                    ChangedBy = h.ChangedBy,
                    ChangedAt = h.ChangedAt
                })
                .ToList()
        };
    }
}

