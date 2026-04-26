using MediatR;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.Common.Interfaces.Notifications;
using Shopbe.Application.Order.Dtos;
using Shopbe.Domain.Entities.Order;
using Shopbe.Domain.Enums;

namespace Shopbe.Application.Order.Commands.CreateOrder;

public sealed class CreateOrderHandler(IUnitOfWork unitOfWork, IEmailQueue emailQueue) : IRequestHandler<CreateOrderCommand, OrderDetailsDto>
{
    public async Task<OrderDetailsDto> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync();
        try
        {
            var cart = await unitOfWork.Cart.GetByUserIdAsync(request.UserId, cancellationToken);
            if (cart is null || cart.CartItems.Count == 0)
                throw new InvalidOperationException("Cart is empty");

            // Optional: checkout only selected cart items (variantId -> quantity).
            // If empty/null => checkout all cart items.
            var selectedQuantityByVariantId = request.Request.SelectedItems?
                .Where(x => x.ProductVariantId != Guid.Empty)
                .GroupBy(x => x.ProductVariantId)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity))
                ?? new Dictionary<Guid, int>();

            // Filter invalid/zero quantities.
            selectedQuantityByVariantId = selectedQuantityByVariantId
                .Where(kvp => kvp.Value > 0)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            var hasSelection = selectedQuantityByVariantId.Count > 0;

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
                var checkoutQty = item.Quantity;
                if (hasSelection)
                {
                    if (!selectedQuantityByVariantId!.TryGetValue(item.ProductVariantId, out checkoutQty))
                        continue; // Not selected -> keep in cart, skip from order

                    if (checkoutQty > item.Quantity)
                        throw new InvalidOperationException(
                            $"Selected quantity ({checkoutQty}) is greater than quantity in cart ({item.Quantity}) for variant {item.ProductVariantId}");
                }

                var variant = await unitOfWork.ProductVariant.GetProductVariantByIdAsync(item.ProductVariantId);
                if (variant is null)
                    throw new KeyNotFoundException("ProductVariant " + item.ProductVariantId + " not found");

                var unitPrice = variant.Price;
                var totalPrice = unitPrice * checkoutQty;
                subtotal += totalPrice;

                order.OrderItems.Add(new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    ProductVariantId = item.ProductVariantId,
                    SkuSnapshot = variant.Sku,
                    ProductNameSnapshot = variant.Product?.Name ?? string.Empty,
                    Quantity = checkoutQty,
                    UnitPrice = unitPrice,
                    TotalPrice = totalPrice
                });
            }

            if (order.OrderItems.Count == 0)
                throw new InvalidOperationException("No items selected for checkout");

            order.SubtotalAmount = subtotal;

            // Apply coupon (if provided)
            var couponCode = request.Request.CouponCode?.Trim();
            if (!string.IsNullOrWhiteSpace(couponCode))
            {
                var coupon = await unitOfWork.Coupons.GetByCodeAsync(couponCode!, cancellationToken);
                if (coupon is null)
                    throw new KeyNotFoundException("Coupon not found");
                if (!coupon.IsActive)
                    throw new InvalidOperationException("Coupon is inactive");
                if (coupon.ExpiredAt <= DateTime.UtcNow)
                    throw new InvalidOperationException("Coupon is expired");
                if (subtotal < coupon.MinOrderAmount)
                    throw new InvalidOperationException($"Order subtotal must be at least {coupon.MinOrderAmount} to use this coupon");

                decimal discountAmount;
                switch (coupon.DiscountType)
                {
                    case DiscountType.Percentage:
                        discountAmount = subtotal * (coupon.Value / 100m);
                        break;
                    case DiscountType.FixedAmount:
                        discountAmount = coupon.Value;
                        break;
                    case DiscountType.FreeShipping:
                        // Shipping fee is currently always 0 in this handler.
                        discountAmount = 0m;
                        break;
                    default:
                        throw new InvalidOperationException("Unsupported discount type");
                }

                if (coupon.MaxDiscountAmount.HasValue)
                    discountAmount = Math.Min(discountAmount, coupon.MaxDiscountAmount.Value);

                // Guard against negative totals / over-discount.
                discountAmount = Math.Clamp(discountAmount, 0m, subtotal);

                order.DiscountAmount = discountAmount;

                // Consume coupon usage atomically (usage_count + coupon_usages)
                var consumed = await unitOfWork.Coupons.TryConsumeAsync(
                    coupon.Id,
                    request.UserId,
                    order.Id,
                    DateTime.UtcNow,
                    cancellationToken);

                if (!consumed)
                    throw new InvalidOperationException("Coupon usage limit exceeded");

                order.Coupon = coupon;
            }

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

            // Mutate cart based on selection.
            if (!hasSelection)
            {
                await unitOfWork.Cart.ClearAsync(request.UserId, cancellationToken);
            }
            else
            {
                // Decrement or remove only selected items.
                // Note: cart.CartItems is tracked because it is loaded from EF.
                foreach (var item in cart.CartItems.ToList())
                {
                    if (!selectedQuantityByVariantId!.TryGetValue(item.ProductVariantId, out var checkoutQty))
                        continue;

                    if (checkoutQty == item.Quantity)
                    {
                        await unitOfWork.Cart.RemoveItemAsync(request.UserId, item.ProductVariantId, cancellationToken);
                    }
                    else
                    {
                        // Reduce remaining quantity
                        item.Quantity -= checkoutQty;
                    }
                }
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);

            await unitOfWork.CommitTransactionAsync();

            // Fire-and-forget email (persisted outbox + background job)
            // Note: we enqueue after commit to avoid sending if the transaction fails.
            var user = await unitOfWork.Users.GetUserByIdAsync(request.UserId);
            if (user is not null && !string.IsNullOrWhiteSpace(user.Email))
            {
                var subject = $"Order {order.Id} created";
                var body = $"<p>Hi {System.Net.WebUtility.HtmlEncode(user.FullName)},</p>" +
                           $"<p>Your order <b>{order.Id}</b> has been created successfully.</p>" +
                           $"<p>Total: <b>{order.TotalAmount:N0} {order.Currency}</b></p>";

                await emailQueue.EnqueueAsync(
                    to: user.Email,
                    subject: subject,
                    bodyHtml: body,
                    bodyText: $"Your order {order.Id} has been created. Total: {order.TotalAmount:N0} {order.Currency}",
                    userId: request.UserId,
                    orderId: order.Id,
                    idempotencyKey: $"order-created:{order.Id}",
                    cancellationToken: cancellationToken);
            }

            return order.ToDetailsDto();
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
}



