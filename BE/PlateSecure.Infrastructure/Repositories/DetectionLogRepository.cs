using MongoDB.Bson;
using MongoDB.Driver;
using PlateSecure.Domain.Documents;
using PlateSecure.Domain.Interfaces;
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

    public async Task<IEnumerable<DetectionLog>> GetLogsAsync()
    {
        var filter = Builders<DetectionLog>.Filter.Empty;
        return await dbContext.DetectionLogs.Find(filter).ToListAsync();
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
}