namespace PlateSecure.Application.DTOs;

public sealed record DetectionResponse(
    string Id,
    string? LicensePlate,
    double ConfidenceScore,
    bool IsEntry,
    byte[] ImageData );