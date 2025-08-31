using Microsoft.AspNetCore.Http;

namespace Application.DTOs;

public class CsvMeterReadingUploadDto
{
    public required IFormFile CsvFile { get; set; }
}