namespace PlateSecure.Application.DTOs;

public class ParkingEventUpdateDto
{
    public string? LicensePlate { get; set; }
    public string? EntryGate { get; set; }
    public string? ExitGate { get; set; }
    public bool? IsCheckIn { get; set; }
    public double? Fee { get; set; }
    public bool? IsPaid { get; set; }
}