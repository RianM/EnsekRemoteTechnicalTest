using Application.DTOs;

namespace Application.Interfaces;

public interface IMeterReadingValidator
{
    (CsvMeterReadingRowDto? Data, bool HasValidationErrors, List<MeterReadingUploadErrorDto> Errors) 
        ParseCsvRow(string line, int rowNumber);
    
    Task<(bool HasErrors, List<MeterReadingUploadErrorDto> Errors)> 
        ValidateMeterReadingRowAsync(CsvMeterReadingRowDto row, int rowNumber);
}