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
    public async Task<DetectionResponse> ProcessDetectionsAsync(DetectionRequest request)
    {
        if (request.ImageData.Count != request.ConfidenceScores.Count ||
            request.ImageData.Count != request.LicensePlates.Count)
        {
            throw new ArgumentException("Các danh sách không cùng độ dài");
        }

        for (int i = 0; i < request.ImageData.Count; i++)
        {
            var bytes = request.ImageData[i];
            var log = new DetectionLog
            {
                ImageData = bytes,
                ConfidenceScore = request.ConfidenceScores[i],
                LicensePlate = string.IsNullOrWhiteSpace(request.LicensePlates[i])
                    ? null
                    : request.LicensePlates[i],
            };

            try
            {
                // Lưu ảnh lên GridFS và log
                await detectionLogRepository.InsertDetectionLogAsync(log);
                logger.LogInformation("Saved detection log #{Index}", i);

                if (!string.IsNullOrEmpty(log.LicensePlate))
                {
                    var evt = new ParkingEvent
                    {
                        LicensePlate = log.LicensePlate,
                        EntryGate = request.Gate,
                        IsCheckIn = true,
                        Fee = 5000,
                    };
                    await parkingEventRepository.InsertParkingEventAsync(evt);
                    logger.LogInformation("Created parking event for {Plate}", log.LicensePlate);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error at index {Index}", i);
                throw;
            }
        }

        return new DetectionResponse("Processed successfully");
    }
    
    public async Task<IEnumerable<DetectionLog>> GetLogsAsync()
    {
        return await detectionLogRepository.GetLogsAsync();
    }

    public async Task<IEnumerable<ParkingEvent>> GetParkingEventsAsync()
    {
        return await parkingEventRepository.GetAllAsync();
    }
    
    public async Task<ParkingEventResponse> CheckOutAsync(ExitRequest dto)
    {
        // Tìm event check‑in gần nhất
        var last = await parkingEventRepository.GetLatestEventByLicensePlateAsync(dto.LicensePlate);
        if (last == null || !last.IsCheckIn || !string.IsNullOrEmpty(last.ExitGate))
            throw new InvalidOperationException("Không tìm thấy sự kiện check‑in mở cho biển số này.");

        // Cập nhật exit gate & chuyển trạng thái
        last.ExitGate = dto.ExitGate;
        last.IsCheckIn = false;
        last.Fee = dto.Fee;
        last.IsPaid = true;
        last.UpdateDate = DateTime.UtcNow;
        await parkingEventRepository.UpdateParkingEventAsync(last);

        logger.LogInformation("Checked‑out {Plate} at {Gate}", dto.LicensePlate, dto.ExitGate);

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

        logger.LogInformation("Updated payment for event {Id}: IsPaid={Paid}", id, dto.IsPaid);

        return MapToDto(evt);
    }
    
    private static ParkingEventResponse MapToDto(ParkingEvent e) 
        => new (
        e.Id.ToString(),
        e.LicensePlate,
        e.EntryGate,
        e.ExitGate,
        e.IsCheckIn,
        e.Fee,
        e.IsPaid,
        e.CreateDate,
        e.UpdateDate
    );
}