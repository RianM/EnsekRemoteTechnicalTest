using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using System.Globalization;
using Domain.Constants;

namespace Application.Services.CsvMapping;

public class MeterReadingDateTimeConverter : DefaultTypeConverter
{
    public override object? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new TypeConverterException(this, memberMapData, text, row.Context, "DateTime value cannot be empty");
        }

        foreach (var format in ValidationConstants.MeterReading.DateTimeFormats)
        {
            if (DateTime.TryParseExact(text, format, CultureInfo.InvariantCulture, 
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var dateTime))
            {
                return dateTime;
            }
        }

        throw new TypeConverterException(this, memberMapData, text, row.Context, 
            $"Unable to parse '{text}' as DateTime. Expected formats: {string.Join(", ", ValidationConstants.MeterReading.DateTimeFormats)}");
    }

    public override string? ConvertToString(object? value, IWriterRow row, MemberMapData memberMapData)
    {
        if (value is DateTime dateTime)
        {
            return dateTime.ToString(ValidationConstants.MeterReading.DateTimeFormats[0], CultureInfo.InvariantCulture);
        }

        return base.ConvertToString(value, row, memberMapData);
    }
}