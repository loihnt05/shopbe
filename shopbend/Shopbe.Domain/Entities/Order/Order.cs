using System.Security.Principal;

namespace Shopbe.Domain.Entities
{
    public class Order : BaseEntity
    {
        public Guid UserId { get; set; }
        public decimal TotalItems { get; set; }
        public string Status { get; set; } = string.Empty;
        public Guid ShippingAddressId { get; set; }
    }
}
