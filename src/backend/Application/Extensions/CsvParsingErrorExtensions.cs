using Application.DTOs;
using Application.Interfaces;

public static class CsvParsingErrorExtensions
{
    public static MeterReadingUploadErrorDto ToMeterReadingUploadErrorDto(this CsvParsingError csvError)
    {
        return new MeterReadingUploadErrorDto
        {
            Row = csvError.Row,
            Error = string.IsNullOrEmpty(csvError.Field) ? csvError.ErrorMessage
                   : $"{csvError.Field}: {csvError.ErrorMessage}",
            RawData = csvError.RawData
        };
    }
}