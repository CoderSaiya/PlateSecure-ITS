using MongoDB.Bson;
using PlateSecure.Application.DTOs;
using PlateSecure.Domain.Documents;

namespace PlateSecure.Application.Interfaces;

public interface IDetectionService
{
    Task<DetectionResponse> ProcessDetectionsAsync(DetectionRequest request);
    Task<IEnumerable<DetectionLog>> GetLogsAsync();
    Task<IEnumerable<ParkingEvent>> GetParkingEventsAsync();
    Task<ParkingEventResponse> CheckOutAsync(ExitRequest dto);
    Task<ParkingEventResponse> UpdatePaymentAsync(string id, PaymentUpdateDto dto);
}