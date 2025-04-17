using PlateSecure.Application.DTOs;
using PlateSecure.Domain.Specifications;

namespace PlateSecure.Application.Interfaces;

public interface IDetectionService
{
    Task<IEnumerable<DetectionResponse>> ProcessDetectionsAsync(DetectionRequest request);
    Task<IEnumerable<DetectionResponse>> GetLogsAsync(DetectionLogFilter? request);
    Task<IEnumerable<ParkingEventResponse>> GetParkingEventsAsync(ParkingEventFilter? request);
    Task<ParkingEventResponse> GetEventWithLogsAsync(string objectId);
    Task<ParkingEventResponse> CheckOutAsync(ExitRequest dto);
    Task<ParkingEventResponse> UpdatePaymentAsync(string id, PaymentUpdateDto dto);
    Task<IEnumerable<StatisticsResponse>> GetStatisticsAsync(DateTime? startDate, DateTime? endDate, string groupBy);
    Task UpdateParkingEventAsync(string id, ParkingEventUpdateDto dto);
    Task UpdateDetectionLogAsync(string logId, DetectionLogUpdateDto dto);
    Task DeleteParkingEventAsync(string id);
    Task DeleteDetectionLogAsync(string logId);
}