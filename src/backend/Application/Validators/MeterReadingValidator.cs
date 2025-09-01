using FluentValidation;
using Application.DTOs;
using Domain.Constants;
using Domain.Repositories;

namespace Application.Validators;

public class MeterReadingValidator : AbstractValidator<CsvMeterReadingRowDto>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IMeterReadingRepository _meterReadingRepository;

    public MeterReadingValidator(
        IAccountRepository accountRepository,
        IMeterReadingRepository meterReadingRepository)
    {
        _accountRepository = accountRepository;
        _meterReadingRepository = meterReadingRepository;

        RuleFor(x => x.AccountId)
            .GreaterThan(0)
            .WithMessage(ErrorMessages.MeterReading.InvalidAccountIdFormat);

        RuleFor(x => x.MeterReadValue)
            .InclusiveBetween(ValidationConstants.MeterReading.MinValue, ValidationConstants.MeterReading.MaxValue)
            .WithMessage(ErrorMessages.MeterReading.ValueOutOfRange);

        RuleFor(x => x.AccountId)
            .MustAsync(async (accountId, cancellation) =>
            {
                var account = await _accountRepository.GetByIdAsync(accountId);
                return account != null;
            })
            .WithMessage(x => string.Format(ErrorMessages.MeterReading.AccountNotFound, x.AccountId))
            .When(x => x.AccountId > 0);

        RuleFor(x => x)
            .MustAsync(async (reading, cancellation) =>
            {
                var isDuplicate = await _meterReadingRepository.ExistsByAccountIdAndDateTimeAndValueAsync(
                    reading.AccountId, reading.MeterReadingDateTime, reading.MeterReadValue);
                return !isDuplicate;
            })
            .WithMessage(ErrorMessages.MeterReading.DuplicateEntry)
            .When(x => x.AccountId > 0);

        RuleFor(x => x)
            .MustAsync(async (reading, cancellation) =>
            {
                var latestReading = await _meterReadingRepository.GetLatestByAccountIdAsync(reading.AccountId);
                return latestReading == null || reading.MeterReadingDateTime > latestReading.MeterReadingDateTime;
            })
            .WithMessage(x =>
            {
                var latestReading = _meterReadingRepository.GetLatestByAccountIdAsync(x.AccountId).Result;
                return string.Format(ErrorMessages.MeterReading.ReadingTooOld,
                    x.MeterReadingDateTime.ToString("dd/MM/yyyy HH:mm"),
                    latestReading?.MeterReadingDateTime.ToString("dd/MM/yyyy HH:mm"));
            })
            .When(x => x.AccountId > 0);
    }
}