namespace PlateSecure.Application.DTOs;

public class DetectionLogUpdateDto
{
    public string? LicensePlate { get; set; }
    public double? ConfidenceScore { get; set; }
    public byte[]? ImageData { get; set; }
}