using Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace Application.Interfaces;

public interface IMeterReadingService
{
    Task<IEnumerable<MeterReadingDto>> GetAllMeterReadingsAsync();
    Task<MeterReadingUploadResultDto> ProcessCsvMeterReadingsAsync(IFormFile csvFile);
}