namespace PlateSecure.Application.DTOs;

public sealed record ExitRequest(string LicensePlate, string ExitGate, double Fee);