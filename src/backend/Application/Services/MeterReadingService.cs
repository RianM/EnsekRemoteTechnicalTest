using Application.DTOs;
using Application.Extensions;
using Application.Interfaces;
using Domain.Constants;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using System.Text;
using Domain.Repositories;

namespace Application.Services;

public class MeterReadingService(
    IMeterReadingRepository meterReadingRepository,
    IMeterReadingValidator validator)
    : IMeterReadingService
{
    public async Task<IEnumerable<MeterReadingDto>> GetAllMeterReadingsAsync()
    {
        var meterReadings = await meterReadingRepository.GetAllAsync();
        return meterReadings.Select(meterReading => meterReading.ToDto());
    }
    
    public async Task<MeterReadingUploadResultDto> ProcessCsvMeterReadingsAsync(IFormFile csvFile)
    {
        var result = new MeterReadingUploadResultDto();

        try
        {
            await using var stream = csvFile.OpenReadStream();
            using var reader = new StreamReader(stream, Encoding.UTF8);

            if (!await ValidateHeaderAsync(reader, result))
                return result;

            var validReadings = await ParseAndValidateCsvRowsAsync(reader, result);
            await ProcessValidReadingsAsync(validReadings, result);

            return result;
        }
        catch (Exception ex)
        {
            result.Errors.Add(new MeterReadingUploadErrorDto
            {
                Row = result.TotalProcessed,
                Error = ErrorMessages.CsvProcessing.FileProcessingError + $": {ex.Message}",
                RawData = string.Empty
            });
            return result;
        }
    }

    private static async Task<bool> ValidateHeaderAsync(StreamReader reader, MeterReadingUploadResultDto result)
    {
        var headerLine = await reader.ReadLineAsync();
        if (headerLine == null)
        {
            result.Errors.Add(new MeterReadingUploadErrorDto
            {
                Row = 1,
                Error = ErrorMessages.CsvProcessing.EmptyFile,
                RawData = string.Empty
            });
            return false;
        }

        // Validate expected headers (case-insensitive, exact order)
        var actualHeaders = headerLine.Split(',').Select(h => h.Trim()).Where(h => !string.IsNullOrEmpty(h)).ToArray();

        if (!ValidateHeaders(actualHeaders, ValidationConstants.Csv.MeterReadingHeaders))
        {
            result.Errors.Add(new MeterReadingUploadErrorDto
            {
                Row = 1,
                Error =
                    $"{ErrorMessages.CsvProcessing.InvalidHeaders}. Expected: {string.Join(", ", ValidationConstants.Csv.MeterReadingHeaders)}. Found: {string.Join(", ", actualHeaders)}",
                RawData = headerLine
            });
            return false;
        }

        return true;
    }

    private static bool ValidateHeaders(string[] actual, string[] expected)
    {
        if (actual.Length != expected.Length)
            return false;

        for (int i = 0; i < expected.Length; i++)
        {
            if (!string.Equals(actual[i], expected[i], StringComparison.OrdinalIgnoreCase))
                return false;
        }

        return true;
    }

    private async Task<List<CsvMeterReadingRowDto>> ParseAndValidateCsvRowsAsync(StreamReader reader,
        MeterReadingUploadResultDto result)
    {
        var validReadings = new List<CsvMeterReadingRowDto>();
        var rowNumber = 0;
        string? line;

        while ((line = await reader.ReadLineAsync()) != null)
        {
            rowNumber++;
            result.TotalProcessed++;

            if (string.IsNullOrWhiteSpace(line))
                continue;

            var csvRow = validator.ParseCsvRow(line, rowNumber);
            if (csvRow.HasValidationErrors)
            {
                result.Errors.AddRange(csvRow.Errors);
                result.Failed++;
                continue;
            }

            var validationResult = await validator.ValidateMeterReadingRowAsync(csvRow.Data!, rowNumber);
            if (validationResult.HasErrors)
            {
                result.Errors.AddRange(validationResult.Errors);
                result.Failed++;
                continue;
            }

            validReadings.Add(csvRow.Data!);
        }

        return validReadings;
    }

    private async Task ProcessValidReadingsAsync(List<CsvMeterReadingRowDto> validReadings,
        MeterReadingUploadResultDto result)
    {
        foreach (var reading in validReadings)
        {
            try
            {
                var meterReading = new MeterReading
                {
                    AccountId = reading.AccountId,
                    MeterReadingDateTime = reading.MeterReadingDateTime,
                    MeterReadValue = reading.MeterReadValue
                };

                var createdReading = await meterReadingRepository.AddAsync(meterReading);
                result.SuccessfulReadings.Add(createdReading.ToDto());
                result.Successful++;
            }
            catch (Exception ex)
            {
                result.Errors.Add(new MeterReadingUploadErrorDto
                {
                    Row = reading.RowNumber,
                    AccountId = reading.AccountId,
                    Error = string.Format(ErrorMessages.MeterReading.DatabaseError, ex.Message),
                    RawData =
                        $"{reading.AccountId},{reading.MeterReadingDateTime:dd/MM/yyyy HH:mm},{reading.MeterReadValue}"
                });
                result.Failed++;
            }
        }
    }
}