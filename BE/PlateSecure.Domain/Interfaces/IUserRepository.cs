using PlateSecure.Domain.Documents;

namespace PlateSecure.Domain.Interfaces;

public interface IUserRepository
{
    Task AddUserAsync(User user);
    Task<IEnumerable<User>> GetUsersAsync();
    Task<User?> GetUserByUsernameAsync(string username);
}