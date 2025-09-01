namespace Domain.Constants;

public static class ErrorMessages
{
    public static class CsvProcessing
    {
        public const string NoFileProvided = "No CSV file provided";
        public const string InvalidFileType = "File must be a CSV file";
        public const string FileProcessingError = "File processing error";
    }
    
    public static class MeterReading
    {
        public const string InvalidAccountIdFormat = "Invalid AccountId format";
        public const string ValueOutOfRange = "Reading value must be between 0 and 99999 (NNNNN format)";
        public const string AccountNotFound = "Account with AccountId {0} not found";
        public const string DuplicateEntry = "Duplicate meter reading entry (same AccountId, DateTime, and Value)";
        public const string ReadingTooOld = "New reading date ({0}) must be newer than existing latest reading";
        public const string DatabaseError = "Database error: {0}";
    }
    
    public static class Authentication
    {
        public const string TokenHeaderMessage = "Use this token in Authorization header: Bearer {token}";
    }
}