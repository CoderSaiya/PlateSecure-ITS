using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using PlateSecure.Application.DTOs;
using PlateSecure.Application.Interfaces;
using PlateSecure.Domain.Specifications;

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
                new List<byte[]> { bytes },
                new List<double> { dto.ConfidenceScore },
                dto.LicensePlate,
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
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> HandleExitEvent(
            [FromForm] ExitEventDto dto)
        {
            try
            {
                // Read image bytes
                await using var ms = new MemoryStream();
                await dto.Image.CopyToAsync(ms);
                var bytes = ms.ToArray();
                
                var request = new ExitRequest(
                    new List<byte[]> { bytes }, 
                    new List<double> { dto.ConfidenceScore }, 
                    dto.LicensePlate, 
                    dto.ExitGate, 
                    dto.Fee);
                
                var res = await detectionService.CheckOutAsync(request);
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
        public async Task<IActionResult> GetLogs([FromQuery] DetectionLogFilter? filter)
        {
            var allowedSortFields = new[] { "Id", "CreateDate", "UpdateDate", "LicensePlate", "ConfidenceScore", "ParkingEventId", "IsEntry" };
            if (filter is not null && !allowedSortFields.Contains(filter.SortBy))
                return BadRequest("Invalid sort field");
            
            var logs = await detectionService.GetLogsAsync(filter);
            return Ok(logs);
        }
        
        [HttpGet("events")]
        public async Task<IActionResult> GetEvent([FromQuery] ParkingEventFilter? filter)
        {
            var allowedSortFields = new[] { "Id", "CreateDate", "UpdateDate", "LicensePlate", "EntryGate", "ExitGate", "IsCheckIn", "Fee", "IsPaid" };
            if (filter is not null && !allowedSortFields.Contains(filter.SortBy))
                return BadRequest("Invalid sort field");
            
            var events = await detectionService.GetParkingEventsAsync(filter);
            return Ok(events);
        }
        
        [HttpGet("event/{id}")]
        public async Task<IActionResult> GetPlateEvent(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest("Event id is required.");

            try
            {
                var dto = await detectionService.GetEventWithLogsAsync(id);
                return Ok(dto);
            }
            catch (InvalidOperationException)
            {
                return NotFound($"Không tìm thấy event với id = {id}");
            }
        }
        
        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics(
            [FromQuery] string? startDate,
            [FromQuery] string? endDate,
            [FromQuery] string groupBy)
        {
            var validGroupBys = new[] { "day", "month", "year", "monthofyear" };
            if (!string.IsNullOrEmpty(groupBy) && !validGroupBys.Contains(groupBy.ToLower()))
            {
                return BadRequest("Invalid groupBy value. Allowed values: day, month, year.");
            }
            
            DateTime? parsedStartDate = null;
            DateTime? parsedEndDate = null;
            
            // Hàm phụ trợ để parse ngày theo groupBy
            DateTime? ParseDate(string dateInput, string group)
            {
                if (string.IsNullOrEmpty(dateInput))
                {
                    return null;
                }

                switch (group?.ToLower())
                {
                    case "year":
                        // Định dạng: yyyy
                        if (int.TryParse(dateInput, out int year))
                        {
                            return new DateTime(year, 1, 1);
                        }
                        else
                        {
                            return null;
                        }

                    case "month":
                        // Định dạng: yyyy-MM
                        if (DateTime.TryParseExact(dateInput, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime monthDate))
                        {
                            return monthDate;
                        }
                        else
                        {
                            return null;
                        }
                    
                    case "monthofyear":
                        // Định dạng: yyyy
                        if (DateTime.TryParseExact(dateInput, "yyyy", CultureInfo.InvariantCulture,
                                DateTimeStyles.None, out DateTime yearForMonth))
                        {
                            return yearForMonth;
                        }
                        break;

                    default:
                        // Mặc định: nhập ngày đầy đủ theo định dạng "yyyy-MM-dd"
                        if (DateTime.TryParseExact(dateInput, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                                DateTimeStyles.None, out DateTime dayDate))
                        {
                            return dayDate;
                        }
                        // Nếu không trùng khớp, thử dùng TryParse thông thường
                        if (DateTime.TryParse(dateInput, out DateTime dt))
                        {
                            return dt;
                        }
                        break;
                }
                return null;
            }
            
            if (groupBy == "monthofyear")
            {
                // Đối với groupBy = "monthofyear", bắt buộc phải có startDate dưới dạng năm ("yyyy")
                if (string.IsNullOrEmpty(startDate))
                {
                    return BadRequest("For groupBy 'monthofyear', startDate is required and must contain a year (format: yyyy).");
                }
                parsedStartDate = ParseDate(startDate, groupBy);
                if (parsedStartDate == null)
                {
                    return BadRequest("Invalid startDate format for groupBy 'monthofyear'. Expected 'yyyy'.");
                }
            }
            else
            {
                // Các trường hợp còn lại: "day", "month", "year"
                if (!string.IsNullOrEmpty(startDate))
                {
                    parsedStartDate = ParseDate(startDate, groupBy);
                    if (parsedStartDate == null)
                    {
                        string expectedFormat = groupBy switch
                        {
                            "year" => "yyyy",
                            "month" => "yyyy-MM",
                            _ => "yyyy-MM-dd"
                        };
                        return BadRequest($"Invalid startDate format for groupBy '{groupBy}'. Expected '{expectedFormat}'.");
                    }
                }
            }

            if (!string.IsNullOrEmpty(endDate))
            {
                // Với endDate ta sử dụng cùng định dạng như startDate của các groupBy khác (không áp dụng cho "monthofyear")
                parsedEndDate = ParseDate(endDate, groupBy == "monthofyear" ? "year" : groupBy);
                if (parsedEndDate == null)
                {
                    string expectedFormat = groupBy switch
                    {
                        "year" or "monthofyear" => "yyyy",
                        "month" => "yyyy-MM",
                        _ => "yyyy-MM-dd"
                    };
                    return BadRequest($"Invalid endDate format for groupBy '{groupBy}'. Expected '{expectedFormat}'.");
                }
            }

            var result = await detectionService.GetStatisticsAsync(parsedStartDate, parsedEndDate, groupBy);
            return Ok(result);
        }

        [HttpPut("logs")]
        public async Task<IActionResult> UpdateLog([FromQuery] string id, [FromBody] DetectionLogUpdateDto dto)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest("Id là bắt buộc.");
            
            await detectionService.UpdateDetectionLogAsync(id, dto);
            return Ok("Updated log");
        }
        
        [HttpPut("events")]
        public async Task<IActionResult> UpdateEvent([FromQuery] string id, [FromBody] ParkingEventUpdateDto dto)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest("Id là bắt buộc.");
            
            await detectionService.UpdateParkingEventAsync(id, dto);
            return Ok("Updated event");
        }

        [HttpDelete("logs")]
        public async Task<IActionResult> DeleteLog([FromQuery] string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest("Id là bắt buộc.");
            
            await detectionService.DeleteDetectionLogAsync(id);
            return Ok("Deleted log");
        }
        
        [HttpDelete("events")]
        public async Task<IActionResult> DeleteParkingEvent([FromQuery] string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest("Id là bắt buộc.");
            
            await detectionService.DeleteParkingEventAsync(id);
            return Ok("Deleted events");
        }
    }
}