namespace Application.DTOs;

public class CsvMeterReadingRowDto
{
    public int AccountId { get; set; }
    public DateTime MeterReadingDateTime { get; set; }
    public int MeterReadValue { get; set; }
    public int RowNumber { get; set; }
}