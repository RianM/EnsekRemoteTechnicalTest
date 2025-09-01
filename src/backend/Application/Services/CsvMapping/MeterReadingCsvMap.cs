using CsvHelper.Configuration;
using Application.DTOs;

namespace Application.Services.CsvMapping;

public sealed class MeterReadingCsvMap : ClassMap<CsvMeterReadingRowDto>
{
    public MeterReadingCsvMap()
    {
        Map(m => m.AccountId).Name("AccountId");
        Map(m => m.MeterReadingDateTime).Name("MeterReadingDateTime")
            .TypeConverter<MeterReadingDateTimeConverter>();
        Map(m => m.MeterReadValue).Name("MeterReadValue");
        Map(m => m.RowNumber).Ignore();
    }
}