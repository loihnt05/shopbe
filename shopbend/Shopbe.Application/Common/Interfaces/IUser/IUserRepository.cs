using Shopbe.Domain.Entities.User;

namespace Shopbe.Application.Common.Interfaces.IUser;

public interface IUserRepository
{
    Task<User?> GetUserByIdAsync(Guid id);
    Task<User?> GetUserByKeyCloakIdAsync(string keyCloakId);
    Task<User?> GetUserByEmailAsync(string email);
    Task CreateUserAsync(User user);
    Task UpdateUserAsync(User user);
    Task DeleteUserAsync(Guid id);
    Task<IEnumerable<User>> GetAllUsersAsync();
}