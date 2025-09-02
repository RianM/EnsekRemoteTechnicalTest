using Application.DTOs;
using Application.Extensions;
using Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Domain.Repositories;
using Domain.Entities;
using Domain.Constants;

namespace Application.Services;

public class MeterReadingService : IMeterReadingService
{
    private readonly IMeterReadingRepository _meterReadingRepository;
    private readonly ICsvReader<CsvMeterReadingRowDto> _csvReader;
    private readonly IBatchMeterReadingValidator _batchValidator;

    public MeterReadingService(
        IMeterReadingRepository meterReadingRepository,
        ICsvReader<CsvMeterReadingRowDto> csvReader,
        IBatchMeterReadingValidator batchValidator)
    {
        _meterReadingRepository = meterReadingRepository;
        _csvReader = csvReader;
        _batchValidator = batchValidator;
    }

    public async Task<IEnumerable<MeterReadingDto>> GetAllMeterReadingsAsync()
    {
        var meterReadings = await _meterReadingRepository.GetAllAsync();
        return meterReadings.Select(meterReading => meterReading.ToDto());
    }

    public async Task<MeterReadingUploadResultDto> ProcessCsvMeterReadingsAsync(IFormFile csvFile)
    {
        var result = new MeterReadingUploadResultDto();

        try
        {
            var csvResult = await ParseCsvAsync(csvFile);

            if (csvResult.HasErrors)
            {
                result.Errors.AddRange(csvResult.Errors.Select(csvError => csvError.ToMeterReadingUploadErrorDto()));
                result.Failed = csvResult.Errors.Count;
                return result;
            }

            result.TotalProcessed = csvResult.Records.Count;

            var validationResults = await ValidateRecordsAsync(csvResult.Records);
            var (validRecords, validationErrors) = ProcessValidationResults(validationResults);
            result.Errors.AddRange(validationErrors);

            var (successfulReadings, insertErrors) = await InsertValidRecordsAsync(validRecords);
            result.SuccessfulReadings.AddRange(successfulReadings);
            result.Errors.AddRange(insertErrors);

            result.Successful = successfulReadings.Count;
            result.Failed = validationErrors.Count + insertErrors.Count;

            return result;
        }
        catch (Exception ex)
        {
            result.Errors.Add(new MeterReadingUploadErrorDto
            {
                Error = $"{ErrorMessages.CsvProcessing.FileProcessingError}: {ex.Message}",
                RawData = string.Empty
            });
            return result;
        }
    }

    private async Task<CsvReadResult<CsvMeterReadingRowDto>> ParseCsvAsync(IFormFile csvFile)
    {
        await using var stream = csvFile.OpenReadStream();
        return await _csvReader.ReadAsync(stream);
    }

    private async Task<IDictionary<CsvMeterReadingRowDto, FluentValidation.Results.ValidationResult>> ValidateRecordsAsync(List<CsvMeterReadingRowDto> records)
    {
        return await _batchValidator.ValidateAsync(records);
    }

    private static (List<CsvMeterReadingRowDto> validRecords, List<MeterReadingUploadErrorDto> errors) ProcessValidationResults(
        IDictionary<CsvMeterReadingRowDto, FluentValidation.Results.ValidationResult> validationResults)
    {
        var validRecords = new List<CsvMeterReadingRowDto>();
        var errors = new List<MeterReadingUploadErrorDto>();

        foreach (var kvp in validationResults)
        {
            var record = kvp.Key;
            var validationResult = kvp.Value;

            if (!validationResult.IsValid)
            {
                foreach (var error in validationResult.Errors)
                {
                    errors.Add(new MeterReadingUploadErrorDto
                    {
                        Row = record.RowNumber,
                        AccountId = record.AccountId,
                        Error = error.ErrorMessage,
                        RawData = $"{record.AccountId},{record.MeterReadingDateTime:dd/MM/yyyy HH:mm},{record.MeterReadValue}"
                    });
                }
            }
            else
            {
                validRecords.Add(record);
            }
        }

        return (validRecords, errors);
    }

    private async Task<(List<MeterReadingDto> successfulReadings, List<MeterReadingUploadErrorDto> errors)> InsertValidRecordsAsync(List<CsvMeterReadingRowDto> validRecords)
    {
        var successfulReadings = new List<MeterReadingDto>();
        var errors = new List<MeterReadingUploadErrorDto>();

        if (!validRecords.Any())
            return (successfulReadings, errors);

        try
        {
            var meterReadings = validRecords.Select(CreateMeterReadingEntity).ToList();
            var createdReadings = await _meterReadingRepository.AddRangeAsync(meterReadings);
            successfulReadings.AddRange(createdReadings.Select(r => r.ToDto()));
        }
        catch
        {
            // Fallback to individual processing for error tracking
            foreach (var record in validRecords)
            {
                try
                {
                    var meterReading = CreateMeterReadingEntity(record);
                    var createdReading = await _meterReadingRepository.AddAsync(meterReading);
                    successfulReadings.Add(createdReading.ToDto());
                }
                catch (Exception ex)
                {
                    errors.Add(new MeterReadingUploadErrorDto
                    {
                        Row = record.RowNumber,
                        AccountId = record.AccountId,
                        Error = string.Format(ErrorMessages.MeterReading.DatabaseError, ex.Message),
                        RawData = $"{record.AccountId},{record.MeterReadingDateTime:dd/MM/yyyy HH:mm},{record.MeterReadValue}"
                    });
                }
            }
        }

        return (successfulReadings, errors);
    }

    private static MeterReading CreateMeterReadingEntity(CsvMeterReadingRowDto record)
    {
        return new MeterReading
        {
            AccountId = record.AccountId,
            MeterReadingDateTime = record.MeterReadingDateTime,
            MeterReadValue = record.MeterReadValue
        };
    }
}