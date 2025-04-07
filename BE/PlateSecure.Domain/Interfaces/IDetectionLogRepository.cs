using PlateSecure.Domain.Documents;

namespace PlateSecure.Domain.Interfaces;

public interface IDetectionLogRepository
{
    Task InsertDetectionLogAsync(DetectionLog log);
    Task<IEnumerable<DetectionLog>> GetLogsByLicensePlateAsync(string licensePlate);
    Task<IEnumerable<DetectionLog>> GetLogsAsync();
}