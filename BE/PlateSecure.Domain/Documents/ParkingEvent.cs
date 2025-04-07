using PlateSecure.Domain.Commons;

namespace PlateSecure.Domain.Documents;

public class ParkingEvent : BaseEntity
{
    public string? LicensePlate { get; set; }
    public string EntryGate { get; set; } = null!;
    public string? ExitGate { get; set; }
    public bool IsCheckIn { get; set; } = false;
    public double Fee { get; set; } = 5000.0;
    public bool IsPaid { get; set; } = false;
}