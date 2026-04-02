using Microsoft.EntityFrameworkCore;
using Shopbe.Application.Common.Interfaces.IUser;
using Shopbe.Domain.Entities.User;
using Shopbe.Infrastructure.Persistence;

namespace Shopbe.Infrastructure.Repositories.UserRepository;

public class UserAddressRepository(ShopDbContext context) : IUserAddressRepository
{
    public async Task<UserAddress?> GetUserAddressByIdAsync(Guid id)
    {
        return await context.UserAddresses.FindAsync(id);
    }

    public async Task<IEnumerable<UserAddress>> GetAllUserAddressAsync()
    {
        return await context.UserAddresses.ToListAsync();
    }
    
    public async Task CreateUserAddressAsync(UserAddress userAddress)
    {
        await context.UserAddresses.AddAsync(userAddress);
        await context.SaveChangesAsync();
    }

    public async Task UpdateUserAddressAsync(UserAddress userAddress)
    {
        context.UserAddresses.Update(userAddress);
        await context.SaveChangesAsync();
        
    }

    public async Task DeleteUserAddressAsync(Guid id)
    {
        var userAddressToDelete = await context.UserAddresses.FindAsync(id);
        if (userAddressToDelete != null)
        {
            context.UserAddresses.Remove(userAddressToDelete);
            await context.SaveChangesAsync();
        }
    }
    
}