using MongoDB.Bson;
using PlateSecure.Domain.Documents;
using PlateSecure.Domain.Specifications;

namespace PlateSecure.Domain.Interfaces;

public interface IParkingEventRepository
{
    Task InsertParkingEventAsync(ParkingEvent parkingEvent);
    Task<IEnumerable<ParkingEvent>> GetAllAsync(ParkingEventFilter filterOptions);
    Task<ParkingEvent?> GetLatestEventByLicensePlateAsync(string licensePlate);
    Task<ParkingEvent?> GetByIdAsync(ObjectId id);
    Task UpdateParkingEventAsync(ParkingEvent parkingEvent);
    Task<IEnumerable<ParkingEvent>> GetEventsByDateRangeAsync(DateTime? startDate, DateTime? endDate);
    Task DeleteAsync(ObjectId id);
}