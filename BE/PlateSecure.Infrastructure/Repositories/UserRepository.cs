using MongoDB.Driver;
using PlateSecure.Domain.Documents;
using PlateSecure.Domain.Interfaces;
using PlateSecure.Infrastructure.Persistence;

namespace PlateSecure.Infrastructure.Repositories;

public class UserRepository(MongoDbContext dbContext) : IUserRepository
{
    public async Task AddUserAsync(User user)
    {
        await dbContext.Users.InsertOneAsync(user);
    }

    public async Task<IEnumerable<User>> GetUsersAsync()
    {
        var filter = Builders<User>.Filter.Empty;
        return await dbContext.Users.Find(filter).ToListAsync();
    }
    
    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Username, username);
        return await dbContext.Users.Find(filter).FirstOrDefaultAsync();
    }
}