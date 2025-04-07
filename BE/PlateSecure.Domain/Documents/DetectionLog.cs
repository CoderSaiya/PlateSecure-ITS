using MongoDB.Bson;
using PlateSecure.Domain.Commons;

namespace PlateSecure.Domain.Documents;

public class DetectionLog : BaseEntity
{
    public string? LicensePlate { get; set; }
    public double ConfidenceScore { get; set; } = 100.0;
    public ObjectId? ParkingEventId { get; set; }
    public bool IsEntry { get; set; }
    public byte[] ImageData { get; set; } = Array.Empty<byte>();
}