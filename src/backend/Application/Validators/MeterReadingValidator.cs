using FluentValidation;
using Application.DTOs;
using Domain.Constants;
using Domain.Repositories;

namespace Application.Validators;

public class MeterReadingValidator : AbstractValidator<CsvMeterReadingRowDto>
{
    public MeterReadingValidator(
        IAccountRepository accountRepository,
        IMeterReadingRepository meterReadingRepository)
    {
        var accountRepository1 = accountRepository;
        var meterReadingRepository1 = meterReadingRepository;

        RuleFor(x => x.AccountId)
            .GreaterThan(0)
            .WithMessage(ErrorMessages.MeterReading.InvalidAccountIdFormat);

        RuleFor(x => x.MeterReadValue)
            .InclusiveBetween(ValidationConstants.MeterReading.MinValue, ValidationConstants.MeterReading.MaxValue)
            .WithMessage(ErrorMessages.MeterReading.ValueOutOfRange);

        RuleFor(x => x.AccountId)
            .MustAsync(async (accountId, _) =>
            {
                var account = await accountRepository1.GetByIdAsync(accountId);
                return account != null;
            })
            .WithMessage(x => string.Format(ErrorMessages.MeterReading.AccountNotFound, x.AccountId))
            .When(x => x.AccountId > 0);

        RuleFor(x => x)
            .MustAsync(async (reading, _) =>
            {
                var isDuplicate = await meterReadingRepository1.ExistsByAccountIdAndDateTimeAndValueAsync(
                    reading.AccountId, reading.MeterReadingDateTime, reading.MeterReadValue);
                return !isDuplicate;
            })
            .WithMessage(ErrorMessages.MeterReading.DuplicateEntry)
            .When(x => x.AccountId > 0);

        RuleFor(x => x)
            .MustAsync(async (reading, _) =>
            {
                var latestReading = await meterReadingRepository1.GetLatestByAccountIdAsync(reading.AccountId);
                return latestReading == null || reading.MeterReadingDateTime > latestReading.MeterReadingDateTime;
            })
            .WithMessage(x => string.Format(ErrorMessages.MeterReading.ReadingTooOld,
                x.MeterReadingDateTime.ToString("dd/MM/yyyy HH:mm")))
            .When(x => x.AccountId > 0);
    }
}