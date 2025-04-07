using Microsoft.Extensions.Options;
using MongoDB.Driver;
using PlateSecure.Domain.Documents;
using PlateSecure.Infrastructure.Settings;

namespace PlateSecure.Infrastructure.Persistence;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IOptions<MongoDbSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        _database = client.GetDatabase(settings.Value.DatabaseName);
    }

    public IMongoCollection<DetectionLog> DetectionLogs =>
        _database.GetCollection<DetectionLog>("DetectionLogs");

    public IMongoCollection<ParkingEvent> ParkingEvents =>
        _database.GetCollection<ParkingEvent>("ParkingEvents");
}