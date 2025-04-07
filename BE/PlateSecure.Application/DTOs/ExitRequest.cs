namespace PlateSecure.Application.DTOs;

public sealed record ExitRequest(List<byte[]> ImageData, List<double> ConfidenceScores, string? LicensePlate, string ExitGate, double Fee);