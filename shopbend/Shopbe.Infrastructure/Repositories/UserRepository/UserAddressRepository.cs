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

    public async Task<IEnumerable<UserAddress>> GetUserAddressesByUserIdAsync(Guid userId)
    {
        return await context.UserAddresses
            .Where(x => x.UserId == userId)
            .ToListAsync();
    }

    public async Task UnsetDefaultForUserAsync(Guid userId, Guid? exceptAddressId = null)
    {
        var query = context.UserAddresses
            .Where(x => x.UserId == userId && x.IsDefault);

        if (exceptAddressId.HasValue && exceptAddressId.Value != Guid.Empty)
        {
            query = query.Where(x => x.Id != exceptAddressId.Value);
        }

        // EF Core 7+: ExecuteUpdateAsync. Fall back to in-memory update if not available.
        var addresses = await query.ToListAsync();
        if (addresses.Count == 0) return;

        foreach (var address in addresses)
        {
            address.IsDefault = false;
        }

        await context.SaveChangesAsync();
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