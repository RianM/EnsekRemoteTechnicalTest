using CsvHelper;
using System.Globalization;
using Application.Interfaces;
using Application.DTOs;
using Application.Services.CsvMapping;
using Domain.Constants;

namespace Application.Services;

public class CsvReader<T> : ICsvReader<T> where T : IRowNumber
{
    public async Task<CsvReadResult<T>> ReadAsync(Stream csvStream)
    {
        var result = new CsvReadResult<T>();

        try
        {
            using var reader = new StreamReader(csvStream);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            ConfigureCsvReader(csv);

            await csv.ReadAsync();
            csv.ReadHeader();

            ValidateHeaders(csv.HeaderRecord, result);

            if (result.HasErrors)
                return result;

            var records = new List<T>();
            var rowNumber = 1;

            while (await csv.ReadAsync())
            {
                rowNumber++;

                try
                {
                    var record = csv.GetRecord<T>();
                    record.RowNumber = rowNumber;
                    records.Add(record);
                }
                catch (Exception ex)
                {
                    result.Errors.Add(new CsvParsingError
                    {
                        Row = rowNumber,
                        Field = ex.Data.Contains("FieldName") ? ex.Data["FieldName"]?.ToString() ?? "" : "",
                        Value = ex.Data.Contains("FieldValue") ? ex.Data["FieldValue"]?.ToString() ?? "" : "",
                        ErrorMessage = ex.Message,
                        RawData = csv.Parser.RawRecord
                    });
                }
            }

            result.Records = records;
        }
        catch (Exception ex)
        {
            result.Errors.Add(new CsvParsingError
            {
                ErrorMessage = $"CSV file processing error: {ex.Message}",
            });
        }

        return result;
    }

    private void ConfigureCsvReader(CsvReader csv)
    {
        csv.Context.RegisterClassMap<MeterReadingCsvMap>();

        var config = csv.Context.Configuration;
        config.HasHeaderRecord = true;
        config.HeaderValidated = null;
        config.MissingFieldFound = null;
        config.PrepareHeaderForMatch = args => args.Header.Trim();
    }

    private void ValidateHeaders(string[]? headerRecord, CsvReadResult<T> result)
    {
        if (headerRecord == null || headerRecord.Length == 0)
        {
            result.Errors.Add(new CsvParsingError
            {
                Row = 1,
                ErrorMessage = "CSV file is empty or has no headers",
            });
            return;
        }

        var actualHeaders = headerRecord.Select(h => h.Trim()).Where(h => !string.IsNullOrEmpty(h)).ToArray();
        var expectedHeaders = ValidationConstants.Csv.MeterReadingHeaders;

        if (actualHeaders.Length != expectedHeaders.Length)
        {
            result.Errors.Add(new CsvParsingError
            {
                Row = 1,
                ErrorMessage =
                    $"Invalid number of columns. Expected: {expectedHeaders.Length}, Found: {actualHeaders.Length}",
                RawData = string.Join(",", actualHeaders)
            });
            return;
        }

        for (int i = 0; i < expectedHeaders.Length; i++)
        {
            if (!string.Equals(actualHeaders[i], expectedHeaders[i], StringComparison.OrdinalIgnoreCase))
            {
                result.Errors.Add(new CsvParsingError
                {
                    Row = 1,
                    Field = expectedHeaders[i],
                    Value = actualHeaders[i],
                    ErrorMessage =
                        $"Invalid headers. Expected: {string.Join(", ", expectedHeaders)}. Found: {string.Join(", ", actualHeaders)}",
                    RawData = string.Join(",", actualHeaders)
                });
                return;
            }
        }
    }
}