using MongoDB.Bson;
using PlateSecure.Domain.Documents;
using PlateSecure.Domain.Specifications;

namespace PlateSecure.Domain.Interfaces;

public interface IUserRepository
{
    Task AddUserAsync(User user);
    Task<IEnumerable<User>> GetUsersAsync(UserFilter filterOptions);
    Task<User?> GetUserByUsernameAsync(string username);
    Task<User?> GetByIdAsync(ObjectId id);
    Task UpdateAsync(User user);
    Task DeleteAsync(ObjectId id);
}