namespace Application.DTOs;

public class MeterReadingUploadResultDto
{
    public int TotalProcessed { get; set; }
    public int Successful { get; set; }
    public int Failed { get; set; }
    public List<MeterReadingDto> SuccessfulReadings { get; set; } = new();
    public List<MeterReadingUploadErrorDto> Errors { get; set; } = new();
}