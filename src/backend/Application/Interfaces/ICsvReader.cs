using Application.DTOs;

namespace Application.Interfaces;

public interface ICsvReader<T> where T : class
{
    Task<CsvReadResult<T>> ReadAsync(Stream csvStream);
}

public class CsvReadResult<T>
{
    public List<T> Records { get; set; } = new();
    public List<CsvParsingError> Errors { get; set; } = new();
    public bool HasErrors => Errors.Count > 0;
}

public class CsvParsingError
{
    public int Row { get; set; }
    public string Field { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public string RawData { get; set; } = string.Empty;
}