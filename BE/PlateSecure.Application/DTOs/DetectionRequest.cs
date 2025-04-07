namespace PlateSecure.Application.DTOs;

public sealed record DetectionRequest(List<byte[]> ImageData, List<double> ConfidenceScores, List<string?> LicensePlates, string Gate, bool IsCheckIn);