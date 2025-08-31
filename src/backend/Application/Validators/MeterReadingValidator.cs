using Application.DTOs;
using Application.Interfaces;
using Domain.Constants;
using System.Globalization;
using Domain.Repositories;

namespace Application.Validators;

public class MeterReadingValidator(
    IAccountRepository accountRepository,
    IMeterReadingRepository meterReadingRepository) : IMeterReadingValidator
{
    public (CsvMeterReadingRowDto? Data, bool HasValidationErrors, List<MeterReadingUploadErrorDto> Errors) 
        ParseCsvRow(string line, int rowNumber)
    {
        var errors = new List<MeterReadingUploadErrorDto>();
        var parts = line.Split(',');

        if (parts.Length < ValidationConstants.Csv.MeterReadingMinimumColumns)
        {
            errors.Add(new MeterReadingUploadErrorDto
            {
                Row = rowNumber + 1, // +1 because we skipped header
                Error = ErrorMessages.CsvProcessing.InsufficientColumns,
                RawData = line
            });
            return (null, true, errors);
        }

        // Parse AccountId
        if (!int.TryParse(parts[0]?.Trim(), out var accountId))
        {
            errors.Add(new MeterReadingUploadErrorDto
            {
                Row = rowNumber + 1,
                Error = ErrorMessages.MeterReading.InvalidAccountIdFormat,
                RawData = line
            });
        }

        // Parse MeterReadingDateTime
        DateTime meterReadingDateTime = default;
        var dateTimeString = parts[1]?.Trim();
        if (string.IsNullOrEmpty(dateTimeString) || 
            !DateTime.TryParseExact(dateTimeString, ValidationConstants.MeterReading.DateTimeFormats, 
                                   CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out meterReadingDateTime))
        {
            errors.Add(new MeterReadingUploadErrorDto
            {
                Row = rowNumber + 1,
                AccountId = accountId > 0 ? accountId : null,
                Error = ErrorMessages.MeterReading.InvalidDateTimeFormat,
                RawData = line
            });
        }

        // Parse MeterReadValue
        if (!int.TryParse(parts[2]?.Trim(), out var meterReadValue))
        {
            errors.Add(new MeterReadingUploadErrorDto
            {
                Row = rowNumber + 1,
                AccountId = accountId > 0 ? accountId : null,
                Error = ErrorMessages.MeterReading.InvalidMeterValueFormat,
                RawData = line
            });
        }

        if (errors.Any())
        {
            return (null, true, errors);
        }

        return (new CsvMeterReadingRowDto
        {
            AccountId = accountId,
            MeterReadingDateTime = meterReadingDateTime,
            MeterReadValue = meterReadValue,
            RowNumber = rowNumber + 1
        }, false, errors);
    }

    public async Task<(bool HasErrors, List<MeterReadingUploadErrorDto> Errors)> 
        ValidateMeterReadingRowAsync(CsvMeterReadingRowDto row, int rowNumber)
    {
        var errors = new List<MeterReadingUploadErrorDto>();

        // Validate reading value format (NNNNN - 5 digits, 0-99999)
        if (row.MeterReadValue < ValidationConstants.MeterReading.MinValue || row.MeterReadValue > ValidationConstants.MeterReading.MaxValue)
        {
            errors.Add(new MeterReadingUploadErrorDto
            {
                Row = rowNumber + 1,
                AccountId = row.AccountId,
                Error = ErrorMessages.MeterReading.ValueOutOfRange,
                RawData = $"{row.AccountId},{row.MeterReadingDateTime:dd/MM/yyyy HH:mm},{row.MeterReadValue}"
            });
        }

        // Validate account exists
        var accountExists = await accountRepository.GetByIdAsync(row.AccountId);
        if (accountExists == null)
        {
            errors.Add(new MeterReadingUploadErrorDto
            {
                Row = rowNumber + 1,
                AccountId = row.AccountId,
                Error = string.Format(ErrorMessages.MeterReading.AccountNotFound, row.AccountId),
                RawData = $"{row.AccountId},{row.MeterReadingDateTime:dd/MM/yyyy HH:mm},{row.MeterReadValue}"
            });
        }

        // Check for duplicate entry (same AccountId, DateTime, and Value)
        var isDuplicate = await meterReadingRepository.ExistsByAccountIdAndDateTimeAndValueAsync(
            row.AccountId, row.MeterReadingDateTime, row.MeterReadValue);
        
        if (isDuplicate)
        {
            errors.Add(new MeterReadingUploadErrorDto
            {
                Row = rowNumber + 1,
                AccountId = row.AccountId,
                Error = ErrorMessages.MeterReading.DuplicateEntry,
                RawData = $"{row.AccountId},{row.MeterReadingDateTime:dd/MM/yyyy HH:mm},{row.MeterReadValue}"
            });
        }

        // Check if new reading is older than existing latest reading
        var latestReading = await meterReadingRepository.GetLatestByAccountIdAsync(row.AccountId);
        if (latestReading != null && row.MeterReadingDateTime <= latestReading.MeterReadingDateTime)
        {
            errors.Add(new MeterReadingUploadErrorDto
            {
                Row = rowNumber + 1,
                AccountId = row.AccountId,
                Error = string.Format(ErrorMessages.MeterReading.ReadingTooOld, 
                    row.MeterReadingDateTime.ToString("dd/MM/yyyy HH:mm"), 
                    latestReading.MeterReadingDateTime.ToString("dd/MM/yyyy HH:mm")),
                RawData = $"{row.AccountId},{row.MeterReadingDateTime:dd/MM/yyyy HH:mm},{row.MeterReadValue}"
            });
        }

        return (errors.Any(), errors);
    }
}