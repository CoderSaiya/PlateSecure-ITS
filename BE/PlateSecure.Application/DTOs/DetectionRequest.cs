namespace PlateSecure.Application.DTOs;

public sealed record DetectionRequest(List<byte[]> ImageData, List<double> ConfidenceScores, string? LicensePlate, string Gate, bool IsCheckIn);