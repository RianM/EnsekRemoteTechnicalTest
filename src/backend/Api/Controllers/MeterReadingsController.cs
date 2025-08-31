using Application.DTOs;
using Application.Interfaces;
using Api.Extensions;
using Domain.Constants;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MeterReadingsController(IMeterReadingService meterReadingService) : ControllerBase
{
    [HttpPost("meter-reading-uploads")]
    public async Task<ActionResult<MeterReadingUploadResultDto>> UploadMeterReadingsCsv([FromForm] CsvMeterReadingUploadDto uploadDto)
    {
        if (uploadDto.CsvFile.Length == 0)
        {
            return this.BadRequestWithMessage<MeterReadingUploadResultDto>(ErrorMessages.CsvProcessing.NoFileProvided);
        }

        if (!uploadDto.CsvFile.FileName.EndsWith(ValidationConstants.Csv.FileExtension, StringComparison.OrdinalIgnoreCase))
        {
            return this.BadRequestWithMessage<MeterReadingUploadResultDto>(ErrorMessages.CsvProcessing.InvalidFileType);
        }

        try
        {
            var result = await meterReadingService.ProcessCsvMeterReadingsAsync(uploadDto.CsvFile);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return this.HandleError<MeterReadingUploadResultDto>(ex, "Failed to process CSV meter readings");
        }
    }
}