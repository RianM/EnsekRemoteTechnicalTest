namespace Application.DTOs;

public class MeterReadingUploadErrorDto
{
    public int Row { get; set; }
    public int? AccountId { get; set; }
    public required string Error { get; set; }
    public required string RawData { get; set; }
}