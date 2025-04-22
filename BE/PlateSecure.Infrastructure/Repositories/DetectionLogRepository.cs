using MongoDB.Bson;
using MongoDB.Driver;
using PlateSecure.Domain.Documents;
using PlateSecure.Domain.Interfaces;
using PlateSecure.Domain.Specifications;
using PlateSecure.Infrastructure.Persistence;

namespace PlateSecure.Infrastructure.Repositories;

public class DetectionLogRepository(MongoDbContext dbContext) : IDetectionLogRepository
{
    public async Task InsertDetectionLogAsync(DetectionLog log)
    {
        await dbContext.DetectionLogs.InsertOneAsync(log);
    }

    public async Task<IEnumerable<DetectionLog>> GetLogsByLicensePlateAsync(string licensePlate)
    {
        var filter = Builders<DetectionLog>.Filter.Eq(x => x.LicensePlate, licensePlate);
        return await dbContext.DetectionLogs.Find(filter).ToListAsync();
    }

    public async Task<IEnumerable<DetectionLog>> GetLogsAsync(DetectionLogFilter filterOptions)
    {
        var filter = Builders<DetectionLog>.Filter.Empty;

        if (!string.IsNullOrEmpty(filterOptions.LicensePlate))
            filter &= Builders<DetectionLog>.Filter.Eq(e => e.LicensePlate, filterOptions.LicensePlate);

        if (filterOptions.IsEntry.HasValue)
            filter &= Builders<DetectionLog>.Filter.Eq(e => e.IsEntry, filterOptions.IsEntry.Value);

        if (filterOptions.StartDate.HasValue)
            filter &= Builders<DetectionLog>.Filter.Gte(e => e.CreateDate, filterOptions.StartDate.Value);

        if (filterOptions.EndDate.HasValue)
            filter &= Builders<DetectionLog>.Filter.Lte(e => e.CreateDate, filterOptions.EndDate.Value);

        var sortBuilder = Builders<DetectionLog>.Sort;
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
        
        return await dbContext.DetectionLogs.Find(filter)
            .Sort(sortDefinition)
            .Skip((filterOptions.PageNumber - 1) * filterOptions.PageSize)
            .Limit(filterOptions.PageSize)
            .ToListAsync();
    }
    
    public async Task<DetectionLog?> GetLogByEventIdAndTypeAsync(ObjectId eventId, bool isEntry)
    {
        var filter = Builders<DetectionLog>.Filter.And(
            Builders<DetectionLog>.Filter.Eq(x => x.ParkingEventId, eventId),
            Builders<DetectionLog>.Filter.Eq(x => x.IsEntry, isEntry)
        );
        return await dbContext.DetectionLogs.Find(filter)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<DetectionLog>> GetByEventIdAsync(ObjectId eventId)
    {
        var filter = Builders<DetectionLog>.Filter.Eq(x => x.ParkingEventId, eventId);
        return await dbContext.DetectionLogs.Find(filter).ToListAsync();
    }

    public async Task<DetectionLog?> GetByIdAsync(ObjectId eventId)
    {
        var filter = Builders<DetectionLog>.Filter.Eq(x => x.Id, eventId);
        return await dbContext.DetectionLogs.Find(filter).FirstOrDefaultAsync();
    }

    public async Task UpdateAsync(DetectionLog updatedLog)
    {
        var filter = Builders<DetectionLog>.Filter.Eq(x => x.Id, updatedLog.Id);
        await dbContext.DetectionLogs.ReplaceOneAsync(filter, updatedLog);
    }

    public async Task DeleteAsync(ObjectId id)
    {
        var filter = Builders<DetectionLog>.Filter.Eq(x => x.Id, id);
        await dbContext.DetectionLogs.DeleteOneAsync(filter);
    }
}