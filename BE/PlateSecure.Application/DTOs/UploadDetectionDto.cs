using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PlateSecure.Application.DTOs;

public class UploadDetectionDto
{
    [FromForm] public IFormFile Image { get; set; } = default!;
    [FromForm] public double ConfidenceScore { get; set; }
    [FromForm] public string? LicensePlate { get; set; }
    [FromForm] public string? MetadataJson { get; set; }
}