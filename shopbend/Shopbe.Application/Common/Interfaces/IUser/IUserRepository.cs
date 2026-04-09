namespace Shopbe.Application.Common.Interfaces.IUser;

public interface IUserRepository
{
    Task<Domain.Entities.User.User?> GetUserByIdAsync(Guid id);
    Task<Domain.Entities.User.User?> GetUserByKeycloakIdAsync(string keyCloakId);
    Task<Domain.Entities.User.User?> GetUserByEmailAsync(string email);
    Task CreateUserAsync(Domain.Entities.User.User user);
    Task UpdateUserAsync(Domain.Entities.User.User user);
    Task DeleteUserAsync(Guid id);
    Task<IEnumerable<Domain.Entities.User.User>> GetAllUsersAsync();
}