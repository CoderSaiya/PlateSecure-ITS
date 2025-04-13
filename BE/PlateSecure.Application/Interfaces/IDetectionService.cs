using PlateSecure.Application.DTOs;
using PlateSecure.Domain.Documents;

namespace PlateSecure.Application.Interfaces;

public interface IDetectionService
{
    Task<IEnumerable<DetectionResponse>> ProcessDetectionsAsync(DetectionRequest request);
    Task<IEnumerable<DetectionResponse>> GetLogsAsync();
    Task<IEnumerable<ParkingEventResponse>> GetParkingEventsAsync();
    Task<ParkingEventResponse> GetEventWithLogsAsync(string objectId);
    Task<ParkingEventResponse> CheckOutAsync(ExitRequest dto);
    Task<ParkingEventResponse> UpdatePaymentAsync(string id, PaymentUpdateDto dto);
    Task<IEnumerable<StatisticsResponse>> GetStatisticsAsync(DateTime? startDate, DateTime? endDate, string groupBy);
}