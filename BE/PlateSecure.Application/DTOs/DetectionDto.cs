using Microsoft.AspNetCore.Http;

namespace PlateSecure.Application.DTOs;

public class DetectionDto
{
    public IFormFile Image { get; set; }
    public double ConfidenceScore { get; set; }
    public string? LicensePlate { get; set; }
    public string? GateIn { get; set; }
    public string? GateOut { get; set; }
    public string? MetadataJson { get; set; }
}