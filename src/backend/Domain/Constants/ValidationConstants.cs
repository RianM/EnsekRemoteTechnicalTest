namespace Domain.Constants;

public static class ValidationConstants
{
    public static class MeterReading
    {
        public const int MinValue = 0;
        public const int MaxValue = 99999;
        
        public static readonly string[] DateTimeFormats = 
        {
            "dd/MM/yyyy HH:mm",
            "d/MM/yyyy HH:mm", 
            "dd/M/yyyy HH:mm",
            "d/M/yyyy HH:mm"
        };
    }
    
    public static class Csv
    {
        public static readonly string[] MeterReadingHeaders = { "AccountId", "MeterReadingDateTime", "MeterReadValue" };
        public const int MeterReadingMinimumColumns = 3;
        public const string FileExtension = ".csv";
    }
}