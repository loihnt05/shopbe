using Microsoft.EntityFrameworkCore;
using Shopbe.Application.Common.Interfaces.IUser;
using Shopbe.Domain.Entities.User;
using Shopbe.Infrastructure.Persistence;

namespace Shopbe.Infrastructure.Repositories.UserRepository;

public class UserRepository(ShopDbContext context) : IUserRepository
{
    public async Task<User?> GetUserByIdAsync(Guid userId)
    {
        return await context.Users
            .Include(u => u.SellerProfile)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<User?> GetUserByKeycloakIdAsync(string keyCloakId)
    {
        return await context.Users
            .Include(u => u.SellerProfile)
            .FirstOrDefaultAsync(u => u.KeycloakId == keyCloakId);
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await context.Users
            .Include(u => u.SellerProfile)
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetUserBySellerIdAsync(Guid sellerId)
    {
        return await context.Users
            .Include(u => u.SellerProfile)
            .FirstOrDefaultAsync(u => u.Id == sellerId);
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        return await context.Users
            .Include(u => u.SellerProfile)
            .ToListAsync();
    }

    public async Task CreateUserAsync(User user)
    {
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();
    }

    public async Task UpdateUserAsync(User user)
    {
        var tracked = context.Users.Local.FirstOrDefault(u => u.Id == user.Id);
        if (tracked is not null)
        {
            context.Entry(tracked).CurrentValues.SetValues(user);
        }
        else
        {
            context.Users.Update(user);
        }
        await context.SaveChangesAsync();
    }
    
    public async Task DeleteUserAsync(Guid userId)
    {
        var user = await context.Users.FindAsync(userId);
        if (user != null)
        {
            context.Users.Remove(user);
            await context.SaveChangesAsync();
        }
    }
    
    
}
