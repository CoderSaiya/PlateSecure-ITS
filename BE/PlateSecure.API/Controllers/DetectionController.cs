﻿using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using PlateSecure.Application.DTOs;
using PlateSecure.Application.Interfaces;

namespace PlateSecure.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DetectionController(
        IDetectionService detectionService
        ) : Controller
    {
        [HttpPost("entry")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> HandleEntryEvent(
            [FromForm] DetectionDto dto)
        {
            if ((dto.GateIn is null) == (dto.GateOut is null))
            {
                return BadRequest("gate error");
            }
            // Read image bytes
            await using var ms = new MemoryStream();
            await dto.Image.CopyToAsync(ms);
            var bytes = ms.ToArray();
        
            // Build your request DTO
            var request = new DetectionRequest(
                new List<byte[]>    { bytes },
                new List<double>    { dto.ConfidenceScore },
                new List<string?>   { dto.LicensePlate },
                (dto.GateIn ?? dto.GateOut)!,
                dto.GateOut is not null
            );
        
            // Optionally deserialize metadata
            if (!string.IsNullOrWhiteSpace(dto.MetadataJson))
            {
                var metadata = JsonSerializer.Deserialize<Dictionary<string, object>>(dto.MetadataJson);
                // TODO: do something with metadata
            }
        
            // Process and return result
            var result = await detectionService.ProcessDetectionsAsync(request);
            return Ok(result);
        }
        
        [HttpPost("exit")]
        public async Task<IActionResult> HandleExitEvent(
            [FromForm] ExitRequest dto)
        {
            try
            {
                var res = await detectionService.CheckOutAsync(dto);
                return Ok(res);
            }
            catch (InvalidOperationException ioe)
            {
                return BadRequest(new { error = ioe.Message });
            }
        }
        
        [HttpPut("{id}/payment")]
        public async Task<ActionResult<ParkingEventResponse>> UpdatePayment(
            string id,
            [FromBody] PaymentUpdateDto dto)
        {
            try
            {
                var res = await detectionService.UpdatePaymentAsync(id, dto);
                return Ok(res);
            }
            catch (ArgumentException ae)
            {
                return BadRequest(new { error = ae.Message });
            }
            catch (KeyNotFoundException knf)
            {
                return NotFound(new { error = knf.Message });
            }
        }
        
        [HttpGet("logs")]
        public async Task<IActionResult> GetLogs()
        {
            var logs = await detectionService.GetLogsAsync();
            return Ok(logs);
        }
        
        [HttpGet("event")]
        public async Task<IActionResult> GetEvent()
        {
            var events = await detectionService.GetParkingEventsAsync();
            return Ok(events);
        }
    }
}