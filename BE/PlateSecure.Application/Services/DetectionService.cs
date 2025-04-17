using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using PlateSecure.Application.DTOs;
using PlateSecure.Application.Interfaces;
using PlateSecure.Domain.Documents;
using PlateSecure.Domain.Interfaces;
using PlateSecure.Domain.Specifications;

namespace PlateSecure.Application.Services;

public class DetectionService(
    IDetectionLogRepository detectionLogRepository,
    IParkingEventRepository parkingEventRepository,
    ILogger<DetectionService> logger
    ) : IDetectionService
{
    public async Task<IEnumerable<DetectionResponse>> ProcessDetectionsAsync(DetectionRequest request)
    {
        if (request.ImageData.Count != request.ConfidenceScores.Count)
            throw new ArgumentException("Số lượng ảnh và điểm tin cậy không khớp");

        ObjectId? eventId = null;
        if (!string.IsNullOrWhiteSpace(request.LicensePlate))
        {
            var evt = new ParkingEvent
            {
                LicensePlate = request.LicensePlate,
                EntryGate    = request.Gate,
                IsCheckIn    = true,
                Fee          = -1,
            };
            await parkingEventRepository.InsertParkingEventAsync(evt);
            eventId = evt.Id;
            logger.LogInformation("Đã tạo sự kiện đỗ xe #{EventId} cho {Plate}", evt.Id, request.LicensePlate);
        }

        var responses = new List<DetectionResponse>();

        for (int i = 0; i < request.ImageData.Count; i++)
        {
            var img = request.ImageData[i];
            var score = request.ConfidenceScores[i];

            var log = new DetectionLog
            {
                ImageData      = img,
                ConfidenceScore= score,
                LicensePlate   = request.LicensePlate,
                IsEntry        = !string.IsNullOrWhiteSpace(request.LicensePlate),
                ParkingEventId = eventId
            };

            await detectionLogRepository.InsertDetectionLogAsync(log);
            logger.LogInformation("Nhật ký phát hiện đã lưu #{Index}", i);

            responses.Add(new DetectionResponse(
                log.Id.ToString(),
                request.LicensePlate,
                score,
                log.IsEntry,
                img
            ));
        }

        return responses;
    }
    
    public async Task<IEnumerable<DetectionResponse>> GetLogsAsync(DetectionLogFilter? request)
    {
        var logResponse = new List<DetectionResponse>();
        var filter = request ?? new DetectionLogFilter();
        
        var logs = await detectionLogRepository.GetLogsAsync(filter);
        foreach (var log in logs)
        {
            logResponse.Add(new DetectionResponse(log.Id.ToString(), log.LicensePlate, log.ConfidenceScore, log.IsEntry, log.ImageData));
        }
        return logResponse;
    }

    public async Task<IEnumerable<ParkingEventResponse>> GetParkingEventsAsync(ParkingEventFilter? request)
    {
        var eventResponse = new List<ParkingEventResponse>();
        var filter = request ?? new ParkingEventFilter();
        
        var events = await parkingEventRepository.GetAllAsync(filter);
        foreach (var parkingEvent in events)
        {
            var ids = parkingEvent.Id.ToString();
            var entryLog = await detectionLogRepository.GetLogByEventIdAndTypeAsync(parkingEvent.Id, true);
            var exitLog = await detectionLogRepository.GetLogByEventIdAndTypeAsync(parkingEvent.Id, false);
            eventResponse.Add(new ParkingEventResponse(
                ids, 
                parkingEvent.LicensePlate, 
                parkingEvent.EntryGate, 
                parkingEvent.ExitGate, 
                parkingEvent.IsCheckIn, 
                parkingEvent.Fee,
                parkingEvent.IsPaid,
                parkingEvent.CreateDate,
                parkingEvent.UpdateDate,
                ToResponse(entryLog),
                ToResponse(exitLog)));
        }

        return eventResponse;
    }

    public async Task<ParkingEventResponse> GetEventWithLogsAsync(string objectId)
    {
        ObjectId.TryParse(objectId, out var id);
        
        var existingEvent = await parkingEventRepository.GetByIdAsync(id);
        if (existingEvent is null) 
            throw new InvalidOperationException("Không tìm thấy event");

        var entryLog = await detectionLogRepository.GetLogByEventIdAndTypeAsync(id, true);
        var exitLog = await detectionLogRepository.GetLogByEventIdAndTypeAsync(id, false);
        
        return MapToDto(existingEvent, ToResponse(entryLog), ToResponse(exitLog));
    }
    
    public async Task<ParkingEventResponse> CheckOutAsync(ExitRequest request)
    {
        if (request.ImageData.Count != request.ConfidenceScores.Count)
            throw new ArgumentException("Số lượng ảnh và điểm tin cậy không khớp");
        
        // Tìm event check‑in gần nhất
        var last = await parkingEventRepository.GetLatestEventByLicensePlateAsync(request.LicensePlate);
        if (last == null || !last.IsCheckIn || !string.IsNullOrEmpty(last.ExitGate))
            throw new InvalidOperationException("Không tìm thấy sự kiện check‑in mở cho biển số này.");

        // Cập nhật exit gate & chuyển trạng thái
        last.ExitGate = request.ExitGate;
        last.IsCheckIn = false;
        last.Fee = request.Fee;
        last.IsPaid = true;
        last.UpdateDate = DateTime.UtcNow;
        await parkingEventRepository.UpdateParkingEventAsync(last);

        logger.LogInformation("Đã kiểm tra {Plate} tại {Gate}", request.LicensePlate, request.ExitGate);

        return MapToDto(last);
    }

    public async Task<ParkingEventResponse> UpdatePaymentAsync(string id, PaymentUpdateDto dto)
    {
        if (!ObjectId.TryParse(id, out var objId))
            throw new ArgumentException("Id không hợp lệ.");

        var evt = await parkingEventRepository.GetByIdAsync(objId);
        if (evt == null)
            throw new KeyNotFoundException("Không tìm thấy sự kiện với Id này.");

        evt.IsPaid = dto.IsPaid;
        evt.UpdateDate = DateTime.UtcNow;
        await parkingEventRepository.UpdateParkingEventAsync(evt);

        logger.LogInformation("Cập nhật thanh toán cho sự kiện {Id}: IsPaid={Paid}", id, dto.IsPaid);

        return MapToDto(evt);
    }
    
    private static ParkingEventResponse MapToDto(ParkingEvent e, DetectionResponse? entryLog = null, DetectionResponse? exitLog = null) 
        => new (
        e.Id.ToString(),
        e.LicensePlate,
        e.EntryGate,
        e.ExitGate,
        e.IsCheckIn,
        e.Fee,
        e.IsPaid,
        e.CreateDate,
        e.UpdateDate,
        entryLog,
        exitLog
    );

    private static DetectionResponse ToResponse(DetectionLog? log)
    {
        if(log is null) return null;
        return new DetectionResponse(
            log.Id.ToString(),
            log.LicensePlate,
            log.ConfidenceScore,
            log.IsEntry,
            log.ImageData
        );
    }
    
    public async Task<IEnumerable<StatisticsResponse>> GetStatisticsAsync(DateTime? startDate, DateTime? endDate, string groupBy)
    {
        var events = await parkingEventRepository.GetEventsByDateRangeAsync(startDate, endDate);
        
        if (startDate.HasValue)
            events = events.Where(e => e.CreateDate >= startDate.Value);
        if (endDate.HasValue)
            events = events.Where(e => e.CreateDate <= endDate.Value);
        
        IEnumerable<StatisticsResponse> statistics;
        
        // Mặc định nếu groupBy không được truyền thì group theo ngày
        groupBy = string.IsNullOrWhiteSpace(groupBy) ? "day" : groupBy.ToLower();
        
        switch (groupBy)
        {
            case "day":
                statistics = events.GroupBy(e => e.CreateDate.Date)
                    .Select(g => new StatisticsResponse
                    {
                        Period = g.Key.ToString("yyyy-MM-dd"),
                        TotalEvents = g.Count(),
                        TotalRevenue = g.Sum(e => e.Fee)
                    });
                break;
            case "month":
                statistics = events.GroupBy(e => new { e.CreateDate.Year, e.CreateDate.Month })
                    .Select(g => new StatisticsResponse
                    {
                        Period = $"{g.Key.Year}-{g.Key.Month:D2}",
                        TotalEvents = g.Count(),
                        TotalRevenue = g.Sum(e => e.Fee)
                    });
                break;
            case "year":
                statistics = events.GroupBy(e => e.CreateDate.Year)
                    .Select(g => new StatisticsResponse
                    {
                        Period = g.Key.ToString(),
                        TotalEvents = g.Count(),
                        TotalRevenue = g.Sum(e => e.Fee)
                    });
                break;
            case "monthofyear":
                
            {
                // Nếu không có startDate để xác định năm thì trả về lỗi
                if (!startDate.HasValue)
                {
                    throw new ArgumentException("Để thống kê theo từng tháng của năm, bạn cần truyền startDate chứa năm.");
                }

                int targetYear = startDate.Value.Year;
                // Lọc các sự kiện chỉ trong năm đã chỉ định
                var eventsForYear = events.Where(e => e.CreateDate.Year == targetYear);
                statistics = eventsForYear.GroupBy(e => e.CreateDate.Month)
                    .Select(g => new StatisticsResponse
                    {
                        Period = $"{targetYear}-{g.Key:D2}",
                        TotalEvents = g.Count(),
                        TotalRevenue = g.Sum(e => e.Fee)
                    });
            }
                break;
            default:
                statistics = Enumerable.Empty<StatisticsResponse>();
                break;
        }
        
        return statistics.OrderBy(s => s.Period);
    }

    public async Task UpdateParkingEventAsync(string id, ParkingEventUpdateDto dto)
    {
        if (!ObjectId.TryParse(id, out var objId))
            throw new ArgumentException("ID không hợp lệ");

        var existingEvent = await parkingEventRepository.GetByIdAsync(objId);
        if (existingEvent is null)
            throw new KeyNotFoundException("Không tìm thấy sự kiện");
        
        existingEvent.EntryGate = dto.EntryGate ?? existingEvent.EntryGate;
        existingEvent.ExitGate = dto.ExitGate ?? existingEvent.ExitGate;
        existingEvent.Fee = dto.Fee ?? existingEvent.Fee;
        existingEvent.IsPaid = dto.IsPaid ?? existingEvent.IsPaid;
        existingEvent.IsCheckIn = dto.IsCheckIn ?? existingEvent.IsCheckIn;
        existingEvent.UpdateDate = DateTime.UtcNow;

        // Cập nhật LicensePlate nếu có và đồng bộ với DetectionLog
        if (!string.IsNullOrEmpty(dto.LicensePlate) && dto.LicensePlate != existingEvent.LicensePlate)
        {
            var logs = await detectionLogRepository.GetByEventIdAsync(existingEvent.Id);
            foreach (var log in logs)
            {
                log.LicensePlate = dto.LicensePlate;
                await detectionLogRepository.UpdateAsync(log);
            }
            existingEvent.LicensePlate = dto.LicensePlate;
        }

        await parkingEventRepository.UpdateParkingEventAsync(existingEvent);
        logger.LogInformation("Cập nhật sự kiện #{EventId}", id);
    }

    public async Task UpdateDetectionLogAsync(string logId, DetectionLogUpdateDto dto)
    {
        if (!ObjectId.TryParse(logId, out var objId))
            throw new ArgumentException("ID log không hợp lệ");

        var log = await detectionLogRepository.GetByIdAsync(objId);
        if (log is null)
            throw new KeyNotFoundException("Không tìm thấy log");
        
        log.ConfidenceScore = dto.ConfidenceScore ?? log.ConfidenceScore;
        log.ImageData = dto.ImageData ?? log.ImageData;

        if (!string.IsNullOrEmpty(dto.LicensePlate) && dto.LicensePlate != log.LicensePlate)
        {
            log.LicensePlate = dto.LicensePlate;
            if (log.ParkingEventId.HasValue)
            {
                var parkingEvent = await parkingEventRepository.GetByIdAsync(log.ParkingEventId.Value);
                if (parkingEvent != null)
                {
                    parkingEvent.LicensePlate = dto.LicensePlate;
                    await parkingEventRepository.UpdateParkingEventAsync(parkingEvent);
                }
            }
        }

        await detectionLogRepository.UpdateAsync(log);
        logger.LogInformation("Cập nhật log #{LogId}", logId);
    }

    public async Task DeleteParkingEventAsync(string id)
    {
        if (!ObjectId.TryParse(id, out var objId))
            throw new ArgumentException("ID không hợp lệ");

        var existingEvent = await parkingEventRepository.GetByIdAsync(objId);
        if (existingEvent == null)
            throw new KeyNotFoundException("Không tìm thấy sự kiện");

        // Xóa các DetectionLog liên quan
        var logs = await detectionLogRepository.GetByEventIdAsync(objId);
        foreach (var log in logs)
        {
            await detectionLogRepository.DeleteAsync(log.Id);
        }

        await parkingEventRepository.DeleteAsync(objId);
        logger.LogInformation("Đã xóa sự kiện #{EventId}", id);
    }

    public async Task DeleteDetectionLogAsync(string logId)
    {
        if (!ObjectId.TryParse(logId, out var objId))
            throw new ArgumentException("ID log không hợp lệ");

        var log = await detectionLogRepository.GetByIdAsync(objId);
        if (log == null)
            throw new KeyNotFoundException("Không tìm thấy log");
        
        if (log.ParkingEventId.HasValue)
        {
            var eventLogs = await detectionLogRepository.GetByEventIdAsync(log.ParkingEventId.Value);
            if (eventLogs.Count() == 1)
            {
                await parkingEventRepository.DeleteAsync(log.ParkingEventId.Value);
            }
        }

        await detectionLogRepository.DeleteAsync(objId);
        logger.LogInformation("Đã xóa log #{LogId}", logId);
    }
}