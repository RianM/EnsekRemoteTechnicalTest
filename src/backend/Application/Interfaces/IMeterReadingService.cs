using Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace Application.Interfaces;

public interface IMeterReadingService
{
    Task<MeterReadingUploadResultDto> ProcessCsvMeterReadingsAsync(IFormFile csvFile);
}