using MongoDB.Bson;
using PlateSecure.Domain.Documents;

namespace PlateSecure.Domain.Interfaces;

public interface IParkingEventRepository
{
    Task InsertParkingEventAsync(ParkingEvent parkingEvent);
    Task<IEnumerable<ParkingEvent>> GetAllAsync();
    Task<ParkingEvent?> GetLatestEventByLicensePlateAsync(string licensePlate);
    Task<ParkingEvent?> GetByIdAsync(ObjectId id);
    Task UpdateParkingEventAsync(ParkingEvent parkingEvent);
}