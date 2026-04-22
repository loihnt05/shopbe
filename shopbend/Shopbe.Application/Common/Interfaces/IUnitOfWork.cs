using Shopbe.Application.Common.Interfaces.IBrand;
using Shopbe.Application.Common.Interfaces.ICategory;
using Shopbe.Application.Common.Interfaces.IOrder;
using Shopbe.Application.Common.Interfaces.IProduct;
using Shopbe.Application.Common.Interfaces.IShipping;
using Shopbe.Application.Common.Interfaces.IShoppingCart;
using Shopbe.Application.Common.Interfaces.IUser;
using Shopbe.Application.Common.Interfaces.IPayment;
using Shopbe.Application.Common.Interfaces.IReview;
using Shopbe.Application.Common.Interfaces.IWishlist;

namespace Shopbe.Application.Common.Interfaces;
public interface IUnitOfWork
{
    
    // Users
    IUserRepository Users { get; }
    IUserAddressRepository UserAddresses { get; }
    
    // Categories
    ICategoryRepository Category { get; }

    // Brands
    IBrandRepository Brand { get; }
    
    // Products
    IProductRepository Product { get; }
    IProductImageRepository ProductImage { get; }
    IProductVariantRepository ProductVariant { get; }
    IProductVariantAttributeRepository ProductVariantAttribute { get; }
    IProductAttributeRepository ProductAttribute { get; }
    IAttributeValueRepository AttributeValue { get; }

    // Shopping cart
    ICartRepository Cart { get; }

    // Orders
    IOrderRepository Orders { get; }
    ICouponRepository Coupons { get; }

    // Shipping
    IShippingZoneRepository ShippingZones { get; }
    IShippingZoneDistrictRepository ShippingZoneDistricts { get; }
    IShipmentRepository Shipments { get; }

    // Reviews
    IReviewRepository Reviews { get; }

    // Payments
    IPaymentRepository Payments { get; }

    IPaymentTransactionRepository PaymentTransactions { get; }
    IRefundRepository Refunds { get; }
    IIdempotencyKeyRepository IdempotencyKeys { get; }

    // Wishlist
    IWishlistItemRepository WishlistItems { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}