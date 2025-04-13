using MongoDB.Bson;
using MongoDB.Driver;
using PlateSecure.Domain.Documents;
using PlateSecure.Domain.Interfaces;
using PlateSecure.Infrastructure.Persistence;

namespace PlateSecure.Infrastructure.Repositories;

public class ParkingEventRepository(MongoDbContext dbContext) : IParkingEventRepository
{
    public async Task InsertParkingEventAsync(ParkingEvent parkingEvent)
    {
        await dbContext.ParkingEvents.InsertOneAsync(parkingEvent);
    }

    public async Task<IEnumerable<ParkingEvent>> GetAllAsync()
    {
        var filter = Builders<ParkingEvent>.Filter.Empty;
        return await dbContext.ParkingEvents.Find(filter).ToListAsync();
    }

    public async Task<ParkingEvent?> GetLatestEventByLicensePlateAsync(string licensePlate)
    {
        var filter = Builders<ParkingEvent>.Filter.Eq(x => x.LicensePlate, licensePlate);
        return await dbContext.ParkingEvents.Find(filter)
            .SortByDescending(x => x.CreateDate)
            .FirstOrDefaultAsync();
    }

    public async Task<ParkingEvent?> GetByIdAsync(ObjectId id)
    {
        var filter = Builders<ParkingEvent>.Filter.Eq(x => x.Id, id);
        return await dbContext.ParkingEvents.Find(filter).FirstOrDefaultAsync();
    }
    
    public Task UpdateParkingEventAsync(ParkingEvent parkingEvent)
    {
        var filter = Builders<ParkingEvent>.Filter.Eq(x => x.Id, parkingEvent.Id);
        return dbContext.ParkingEvents.ReplaceOneAsync(filter, parkingEvent);
    }
    
    public async Task<IEnumerable<ParkingEvent>> GetEventsByDateRangeAsync(DateTime? startDate, DateTime? endDate)
    {
        var filterBuilder = Builders<ParkingEvent>.Filter;
        var filter = FilterDefinition<ParkingEvent>.Empty;

        if (startDate.HasValue)
            filter &= filterBuilder.Gte(e => e.CreateDate, startDate.Value);
        if (endDate.HasValue)
            filter &= filterBuilder.Lte(e => e.CreateDate, endDate.Value);

        return await dbContext.ParkingEvents.Find(filter).ToListAsync();
    }
}