using Microsoft.EntityFrameworkCore;
using Shopbe.Application.Common.Interfaces.IUser;
using Shopbe.Domain.Entities.User;
using Shopbe.Infrastructure.Persistence;

namespace Shopbe.Infrastructure.Repositories.UserRepository;

public class UserRepository(ShopDbContext context) : IUserRepository
{
    public async Task<User?> GetUserByIdAsync(Guid userId)
    {
        // FindAsync only supports primary-key lookups.
        return await context.Users.FindAsync(userId);
    }

    public async Task<User?> GetUserByKeyCloakIdAsync(string keyCloakId)
    {
        // For non-primary-key lookups, use a LINQ query.
        return await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.KeycloakId == keyCloakId);
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        return await context.Users.ToListAsync();
    }

    public async Task CreateUserAsync(User user)
    {
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();
    }

    public async Task UpdateUserAsync(User user)
    {
        context.Users.Update(user);
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