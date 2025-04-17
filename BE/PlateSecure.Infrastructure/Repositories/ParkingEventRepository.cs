using MongoDB.Bson;
using MongoDB.Driver;
using PlateSecure.Domain.Documents;
using PlateSecure.Domain.Interfaces;
using PlateSecure.Domain.Specifications;
using PlateSecure.Infrastructure.Persistence;

namespace PlateSecure.Infrastructure.Repositories;

public class ParkingEventRepository(MongoDbContext dbContext) : IParkingEventRepository
{
    public async Task InsertParkingEventAsync(ParkingEvent parkingEvent)
    {
        await dbContext.ParkingEvents.InsertOneAsync(parkingEvent);
    }

    public async Task<IEnumerable<ParkingEvent>> GetAllAsync(ParkingEventFilter filterOptions)
    {
        var filter = Builders<ParkingEvent>.Filter.Empty;
        
        if (!string.IsNullOrEmpty(filterOptions.LicensePlate))
            filter &= Builders<ParkingEvent>.Filter.Eq(e => e.LicensePlate, filterOptions.LicensePlate);

        if (filterOptions.IsCheckIn.HasValue)
            filter &= Builders<ParkingEvent>.Filter.Eq(e => e.IsCheckIn, filterOptions.IsCheckIn.Value);
        
        if (filterOptions.IsPaid.HasValue)
            filter &= Builders<ParkingEvent>.Filter.Eq(e => e.IsPaid, filterOptions.IsPaid.Value);

        if (filterOptions.StartDate.HasValue)
            filter &= Builders<ParkingEvent>.Filter.Gte(e => e.CreateDate, filterOptions.StartDate.Value);

        if (filterOptions.EndDate.HasValue)
            filter &= Builders<ParkingEvent>.Filter.Lte(e => e.CreateDate, filterOptions.EndDate.Value);
        
        var sortBuilder = Builders<ParkingEvent>.Sort;
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
        
        return await dbContext.ParkingEvents.Find(filter)
            .Sort(sortDefinition)
            .Skip((filterOptions.PageNumber - 1) * filterOptions.PageSize)
            .Limit(filterOptions.PageSize)
            .ToListAsync();
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
    
    public async Task UpdateParkingEventAsync(ParkingEvent parkingEvent)
    {
        var filter = Builders<ParkingEvent>.Filter.Eq(x => x.Id, parkingEvent.Id);
        await dbContext.ParkingEvents.ReplaceOneAsync(filter, parkingEvent);
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

    public async Task DeleteAsync(ObjectId id)
    {
        var filter = Builders<ParkingEvent>.Filter.Eq(x => x.Id, id);
        await dbContext.ParkingEvents.DeleteOneAsync(filter);
    }
}