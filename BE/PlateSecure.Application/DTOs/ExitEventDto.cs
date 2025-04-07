using Microsoft.AspNetCore.Http;

namespace PlateSecure.Application.DTOs;

public class ExitEventDto
{
    public IFormFile Image { get; set; }
    public double ConfidenceScore { get; set; }
    public string LicensePlate { get; set; } = null!;
    public string ExitGate { get; set; } = null!;
    public double Fee { get; set; }
}