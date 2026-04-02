using Shopbe.Domain.Entities.User;

namespace Shopbe.Application.Common.Interfaces.IUser;

public interface IUserAddressRepository
{
    Task<UserAddress?> GetUserAddressByIdAsync(Guid id);
    Task<IEnumerable<UserAddress>> GetAllUserAddressAsync();
    Task CreateUserAddressAsync(UserAddress userAddress);
    Task UpdateUserAddressAsync(UserAddress userAddress);
    Task DeleteUserAddressAsync(Guid id);
}