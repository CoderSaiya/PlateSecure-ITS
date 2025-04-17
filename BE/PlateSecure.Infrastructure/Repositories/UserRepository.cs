using MongoDB.Bson;
using MongoDB.Driver;
using PlateSecure.Domain.Documents;
using PlateSecure.Domain.Interfaces;
using PlateSecure.Domain.Specifications;
using PlateSecure.Infrastructure.Persistence;

namespace PlateSecure.Infrastructure.Repositories;

public class UserRepository(MongoDbContext dbContext) : IUserRepository
{
    public async Task AddUserAsync(User user)
    {
        await dbContext.Users.InsertOneAsync(user);
    }

    public async Task<IEnumerable<User>> GetUsersAsync(UserFilter filterOptions)
    {
        var filter = Builders<User>.Filter.Empty;
        
        if (!string.IsNullOrEmpty(filterOptions.Username))
            filter &= Builders<User>.Filter.Eq(e => e.Username, filterOptions.Username);
        
        if (!string.IsNullOrEmpty(filterOptions.Password))
            filter &= Builders<User>.Filter.Eq(e => e.PasswordHash, filterOptions.Password);
        
        if (!string.IsNullOrEmpty(filterOptions.Role))
            filter &= Builders<User>.Filter.Eq(e => e.Role, filterOptions.Role);

        if (filterOptions.StartDate.HasValue)
            filter &= Builders<User>.Filter.Gte(e => e.CreateDate, filterOptions.StartDate.Value);

        if (filterOptions.EndDate.HasValue)
            filter &= Builders<User>.Filter.Lte(e => e.CreateDate, filterOptions.EndDate.Value);
        
        var sortBuilder = Builders<User>.Sort;
        var sortField = filterOptions.SortBy ?? "CreateDate";
        var sortDirection = filterOptions.SortDirection?.ToLower() switch
        {
            "asc"  => 1,
            "desc" => -1,
            _      => 1
        };
        
        var sortDefinition = sortDirection == 1 
            ? sortBuilder.Ascending(sortField)
            : sortBuilder.Descending(sortField);
        
        return await dbContext.Users.Find(filter)
            .Sort(sortDefinition)
            .Skip((filterOptions.PageNumber - 1) * filterOptions.PageSize)
            .Limit(filterOptions.PageSize)
            .ToListAsync();
    }
    
    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Username, username);
        return await dbContext.Users.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<User?> GetByIdAsync(ObjectId id)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Id, id);
        return await dbContext.Users.Find(filter).FirstOrDefaultAsync();
    }

    public async Task UpdateAsync(User user)
    {
        var filter = Builders<User>.Filter.Eq(x => x.Id, user.Id);
        await dbContext.Users.ReplaceOneAsync(filter, user);
    }

    public async Task DeleteAsync(ObjectId id)
    {
        var filter = Builders<ParkingEvent>.Filter.Eq(x => x.Id, id);
        await dbContext.ParkingEvents.DeleteOneAsync(filter);
    }
}