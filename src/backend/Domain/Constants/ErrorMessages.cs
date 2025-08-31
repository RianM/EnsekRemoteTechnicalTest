namespace Domain.Constants;

public static class ErrorMessages
{
    public static class CsvProcessing
    {
        public const string EmptyFile = "CSV file is empty or invalid";
        public const string NoFileProvided = "No CSV file provided";
        public const string InvalidFileType = "File must be a CSV file";
        public const string InvalidHeaders = "Invalid CSV headers";
        public const string InsufficientColumns = "Insufficient columns in CSV row";
        public const string FileProcessingError = "File processing error";
    }
    
    public static class MeterReading
    {
        public const string InvalidAccountIdFormat = "Invalid AccountId format";
        public const string InvalidDateTimeFormat = "Invalid MeterReadingDateTime format (expected: dd/MM/yyyy HH:mm)";
        public const string InvalidMeterValueFormat = "Invalid MeterReadValue format";
        public const string ValueOutOfRange = "Reading value must be between 0 and 99999 (NNNNN format)";
        public const string AccountNotFound = "Account with AccountId {0} not found";
        public const string DuplicateEntry = "Duplicate meter reading entry (same AccountId, DateTime, and Value)";
        public const string ReadingTooOld = "New reading date ({0}) must be newer than existing latest reading ({1})";
        public const string DatabaseError = "Database error: {0}";
    }
}