using FluentValidation;
using FluentValidation.Results;
using Application.DTOs;
using Application.Interfaces;
using Domain.Constants;
using Domain.Entities;
using Domain.Repositories;

namespace Application.Validators;

public class BatchMeterReadingValidator : IBatchMeterReadingValidator
{
    private readonly IAccountRepository _accountRepository;
    private readonly IMeterReadingRepository _meterReadingRepository;

    public BatchMeterReadingValidator(
        IAccountRepository accountRepository,
        IMeterReadingRepository meterReadingRepository)
    {
        _accountRepository = accountRepository;
        _meterReadingRepository = meterReadingRepository;
    }

    public async Task<IDictionary<CsvMeterReadingRowDto, ValidationResult>> ValidateAsync(List<CsvMeterReadingRowDto> readings)
    {
        var results = new Dictionary<CsvMeterReadingRowDto, ValidationResult>();

        var accountIds = GetUniqueAccountIds(readings);
        var validAccounts = await _accountRepository.GetByIdsAsync(accountIds);
        var latestReadings = await _meterReadingRepository.GetLatestByAccountIdsAsync(accountIds);
        List<MeterReading> existingReadings = await GetExistingMeterReadings(readings);

        foreach (var reading in readings)
        {
            var validationResult = new ValidationResult();

            ValidateAccountIdFormat(reading, ref validationResult);
            ValidateMeterReadValueFormat(reading, ref validationResult);
            ValidateAccountExists(reading, validAccounts, ref validationResult);
            ValidateDuplicateEntry(reading, existingReadings, ref validationResult);
            ValidateReadingDate(reading, latestReadings, ref validationResult);

            results[reading] = validationResult;
        }

        return results;
    }

    private static List<int> GetUniqueAccountIds(List<CsvMeterReadingRowDto> readings)
    {
        return readings.Select(r => r.AccountId).Distinct().ToList();
    }

    private async Task<List<MeterReading>> GetExistingMeterReadings(List<CsvMeterReadingRowDto> readings)
    {
        var readingTuples = readings
                    .Select(r => (r.AccountId, r.MeterReadingDateTime, r.MeterReadValue))
                    .ToList();
        var existingReadings = await _meterReadingRepository.GetExistingReadingsAsync(readingTuples);
        return existingReadings;
    }

    private void ValidateAccountIdFormat(CsvMeterReadingRowDto reading, ref ValidationResult validationResult)
    {
        if (reading.AccountId <= 0)
        {
            validationResult.Errors.Add(new ValidationFailure(nameof(reading.AccountId),
                ErrorMessages.MeterReading.InvalidAccountIdFormat));
        }
    }

    private void ValidateMeterReadValueFormat(CsvMeterReadingRowDto reading, ref ValidationResult validationResult)
    {
        if (reading.MeterReadValue < ValidationConstants.MeterReading.MinValue ||
            reading.MeterReadValue > ValidationConstants.MeterReading.MaxValue)
        {
            validationResult.Errors.Add(new ValidationFailure(nameof(reading.MeterReadValue),
                ErrorMessages.MeterReading.ValueOutOfRange));
        }
    }

    private void ValidateAccountExists(CsvMeterReadingRowDto reading, IEnumerable<Account> validAccounts, ref ValidationResult validationResult)
    {
        if (validAccounts.FirstOrDefault(x => x.AccountId == reading.AccountId) == null)
        {
            validationResult.Errors.Add(new ValidationFailure(nameof(reading.AccountId),
                string.Format(ErrorMessages.MeterReading.AccountNotFound, reading.AccountId)));
        }
    }

    private void ValidateDuplicateEntry(CsvMeterReadingRowDto reading, List<MeterReading> existingReadings, ref ValidationResult validationResult)
    {
        if (existingReadings.FirstOrDefault(f => f.AccountId == reading.AccountId && f.MeterReadingDateTime == reading.MeterReadingDateTime && f.MeterReadValue == reading.MeterReadValue) != null)
        {
            validationResult.Errors.Add(new ValidationFailure("", ErrorMessages.MeterReading.DuplicateEntry));
        }
    }

    private void ValidateReadingDate(CsvMeterReadingRowDto reading, IDictionary<int, MeterReading> latestReadings, ref ValidationResult validationResult)
    {
        if (latestReadings.TryGetValue(reading.AccountId, out var latestReading))
        {
            if (reading.MeterReadingDateTime <= latestReading.MeterReadingDateTime)
            {
                validationResult.Errors.Add(new ValidationFailure("",
                    string.Format(ErrorMessages.MeterReading.ReadingTooOld,
                        reading.MeterReadingDateTime.ToString("dd/MM/yyyy HH:mm"))));
            }
        }
    }
}