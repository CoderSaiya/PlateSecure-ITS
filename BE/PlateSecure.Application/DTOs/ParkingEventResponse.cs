namespace PlateSecure.Application.DTOs;

public sealed record ParkingEventResponse(
    string Id, 
    string? LicensePlate, 
    string EntryGate, 
    string? ExitGate, 
    bool IsCheckIn, 
    double Fee, 
    bool IsPaid, 
    DateTime CreateDate, 
    DateTime UpdateDate,
    DetectionResponse? EntryLog,
    DetectionResponse? ExitLog);