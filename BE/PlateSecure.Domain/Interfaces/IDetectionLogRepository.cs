using MongoDB.Bson;
using PlateSecure.Domain.Documents;
using PlateSecure.Domain.Specifications;

namespace PlateSecure.Domain.Interfaces;

public interface IDetectionLogRepository
{
    Task InsertDetectionLogAsync(DetectionLog log);
    Task<IEnumerable<DetectionLog>> GetLogsByLicensePlateAsync(string licensePlate);
    Task<IEnumerable<DetectionLog>> GetLogsAsync(DetectionLogFilter filterOptions);
    Task<DetectionLog?> GetLogByEventIdAndTypeAsync(ObjectId eventId, bool isEntry);
    Task<IEnumerable<DetectionLog>> GetByEventIdAsync(ObjectId eventId);
    Task<DetectionLog?> GetByIdAsync(ObjectId eventId);
    Task UpdateAsync(DetectionLog updatedLog);
    Task DeleteAsync(ObjectId id);
}