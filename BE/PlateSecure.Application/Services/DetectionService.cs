using System.Collections;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using PlateSecure.Application.DTOs;
using PlateSecure.Application.Interfaces;
using PlateSecure.Domain.Documents;
using PlateSecure.Domain.Interfaces;

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
    
    public async Task<IEnumerable<DetectionResponse>> GetLogsAsync()
    {
        var logResponse = new List<DetectionResponse>();
        var logs = await detectionLogRepository.GetLogsAsync();
        foreach (var log in logs)
        {
            logResponse.Add(new DetectionResponse(log.Id.ToString(), log.LicensePlate, log.ConfidenceScore, log.IsEntry, log.ImageData));
        }
        return logResponse;
    }

    public async Task<IEnumerable<ParkingEventResponse>> GetParkingEventsAsync()
    {
        var eventResponse = new List<ParkingEventResponse>();
        var events = await parkingEventRepository.GetAllAsync();
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
}