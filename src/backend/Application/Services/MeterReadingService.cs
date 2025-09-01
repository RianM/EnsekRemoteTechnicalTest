using FluentValidation;
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
    private readonly IValidator<CsvMeterReadingRowDto> _validator;

    public MeterReadingService(
        IMeterReadingRepository meterReadingRepository,
        ICsvReader<CsvMeterReadingRowDto> csvReader,
        IValidator<CsvMeterReadingRowDto> validator)
    {
        _meterReadingRepository = meterReadingRepository;
        _csvReader = csvReader;
        _validator = validator;
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
            // Parse CSV file
            await using var stream = csvFile.OpenReadStream();
            var csvResult = await _csvReader.ReadAsync(stream);

            // Handle CSV parsing errors
            if (csvResult.HasErrors)
            {
                foreach (var csvError in csvResult.Errors)
                {
                    result.Errors.Add(new MeterReadingUploadErrorDto
                    {
                        Row = csvError.Row,
                        AccountId = null,
                        Error = string.IsNullOrEmpty(csvError.Field) ? csvError.ErrorMessage 
                               : $"{csvError.Field}: {csvError.ErrorMessage}",
                        RawData = csvError.RawData
                    });
                }
                result.Failed = csvResult.Errors.Count;
                return result;
            }

            result.TotalProcessed = csvResult.Records.Count;

            // Validate and process each record
            foreach (var record in csvResult.Records)
            {
                var validationResult = await _validator.ValidateAsync(record);

                if (!validationResult.IsValid)
                {
                    foreach (var error in validationResult.Errors)
                    {
                        result.Errors.Add(new MeterReadingUploadErrorDto
                        {
                            Row = record.RowNumber,
                            AccountId = record.AccountId > 0 ? record.AccountId : null,
                            Error = error.ErrorMessage,
                            RawData = $"{record.AccountId},{record.MeterReadingDateTime:dd/MM/yyyy HH:mm},{record.MeterReadValue}"
                        });
                    }
                    result.Failed++;
                    continue;
                }

                // Save valid record
                try
                {
                    var meterReading = new MeterReading
                    {
                        AccountId = record.AccountId,
                        MeterReadingDateTime = record.MeterReadingDateTime,
                        MeterReadValue = record.MeterReadValue
                    };

                    var createdReading = await _meterReadingRepository.AddAsync(meterReading);
                    result.SuccessfulReadings.Add(createdReading.ToDto());
                    result.Successful++;
                }
                catch (Exception ex)
                {
                    result.Errors.Add(new MeterReadingUploadErrorDto
                    {
                        Row = record.RowNumber,
                        AccountId = record.AccountId,
                        Error = string.Format(ErrorMessages.MeterReading.DatabaseError, ex.Message),
                        RawData = $"{record.AccountId},{record.MeterReadingDateTime:dd/MM/yyyy HH:mm},{record.MeterReadValue}"
                    });
                    result.Failed++;
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            result.Errors.Add(new MeterReadingUploadErrorDto
            {
                Row = 0,
                AccountId = null,
                Error = $"{ErrorMessages.CsvProcessing.FileProcessingError}: {ex.Message}",
                RawData = string.Empty
            });
            return result;
        }
    }
}