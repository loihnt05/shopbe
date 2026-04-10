using MediatR;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.Order.Dtos;
using Shopbe.Domain.Entities.Order;
using Shopbe.Domain.Enums;

namespace Shopbe.Application.Order.Commands.CreateOrder;

public sealed class CreateOrderHandler(IUnitOfWork unitOfWork) : IRequestHandler<CreateOrderCommand, OrderDetailsDto>
{
    public async Task<OrderDetailsDto> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync();
        try
        {
            var cart = await unitOfWork.Cart.GetByUserIdAsync(request.UserId, cancellationToken);
            if (cart is null || cart.CartItems.Count == 0)
                throw new InvalidOperationException("Cart is empty");

            // Resolve shipping address:
            // 1) UserAddressId (saved address)
            // 2) Default saved address (if enabled)
            // 3) New/override address fields from request
            var shippingReceiverName = request.Request.ShippingReceiverName;
            var shippingPhone = request.Request.ShippingPhone;
            var shippingAddressLine = request.Request.ShippingAddressLine;
            var shippingCity = request.Request.ShippingCity;
            var shippingDistrict = request.Request.ShippingDistrict;
            var shippingWard = request.Request.ShippingWard;

            if (request.Request.UserAddressId.HasValue && request.Request.UserAddressId.Value != Guid.Empty)
            {
                var addr = await unitOfWork.UserAddresses.GetUserAddressByIdAsync(request.Request.UserAddressId.Value);
                if (addr is null || addr.DeletedAt is not null)
                    throw new KeyNotFoundException("UserAddress not found");
                if (addr.UserId != request.UserId)
                    throw new UnauthorizedAccessException("UserAddress does not belong to current user");

                shippingReceiverName = addr.ReceiverName;
                shippingPhone = addr.Phone;
                shippingAddressLine = addr.AddressLine;
                shippingCity = addr.City;
                shippingDistrict = addr.District;
                shippingWard = addr.Ward;
            }
            else if (request.Request.UseDefaultAddressIfAvailable)
            {
                var addresses = await unitOfWork.UserAddresses.GetUserAddressesByUserIdAsync(request.UserId);
                var defaultAddr = addresses.FirstOrDefault(a => a.IsDefault && a.DeletedAt is null);
                if (defaultAddr is not null)
                {
                    shippingReceiverName = defaultAddr.ReceiverName;
                    shippingPhone = defaultAddr.Phone;
                    shippingAddressLine = defaultAddr.AddressLine;
                    shippingCity = defaultAddr.City;
                    shippingDistrict = defaultAddr.District;
                    shippingWard = defaultAddr.Ward;
                }
            }

            static bool IsShippingInfoMissing(string receiver, string phone, string line, string city, string district, string ward)
                => string.IsNullOrWhiteSpace(receiver)
                   || string.IsNullOrWhiteSpace(phone)
                   || string.IsNullOrWhiteSpace(line)
                   || string.IsNullOrWhiteSpace(city)
                   || string.IsNullOrWhiteSpace(district)
                   || string.IsNullOrWhiteSpace(ward);

            if (IsShippingInfoMissing(shippingReceiverName, shippingPhone, shippingAddressLine, shippingCity, shippingDistrict, shippingWard))
            {
                throw new InvalidOperationException(
                    "Shipping address is required. Provide UserAddressId, ensure a default address exists, or send shipping fields in request.");
            }

            var order = new Domain.Entities.Order.Order
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                ShippingReceiverName = shippingReceiverName,
                ShippingPhone = shippingPhone,
                ShippingAddressLine = shippingAddressLine,
                ShippingCity = shippingCity,
                ShippingDistrict = shippingDistrict,
                ShippingWard = shippingWard,
                Note = request.Request.Note,
                Currency = "VND",
                Status = OrderStatus.Pending,
                DiscountAmount = 0,
                ShippingFee = 0,
            };

            decimal subtotal = 0;
            foreach (var item in cart.CartItems)
            {
                var variant = await unitOfWork.ProductVariant.GetProductVariantByIdAsync(item.ProductVariantId);
                if (variant is null)
                    throw new KeyNotFoundException("ProductVariant " + item.ProductVariantId + " not found");

                var unitPrice = variant.Price;
                var totalPrice = unitPrice * item.Quantity;
                subtotal += totalPrice;

                order.OrderItems.Add(new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    ProductVariantId = item.ProductVariantId,
                    SkuSnapshot = variant.Sku,
                    ProductNameSnapshot = variant.Product?.Name ?? string.Empty,
                    Quantity = item.Quantity,
                    UnitPrice = unitPrice,
                    TotalPrice = totalPrice
                });
            }

            order.SubtotalAmount = subtotal;
            order.TotalAmount = subtotal - order.DiscountAmount + order.ShippingFee;

            order.OrderStatusHistory.Add(new OrderStatusHistory
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                Status = OrderStatus.Pending,
                ChangedBy = request.UserId,
                ChangedAt = DateTime.UtcNow,
                Note = "Order created"
            });

            await unitOfWork.Orders.AddAsync(order, cancellationToken);
            await unitOfWork.Cart.ClearAsync(request.UserId, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            await unitOfWork.CommitTransactionAsync();
            return order.ToDetailsDto();
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
}



