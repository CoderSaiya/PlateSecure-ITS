namespace PlateSecure.Infrastructure.Settings;

public class MongoDbSettings
{
    public string ConnectionString { get; set; } = null!;
    public string DatabaseName { get; set; } = null!;
    public string DetectionLogsCollection { get; set; } = null!;
    public string ParkingEventsCollection { get; set; } = null!;
}