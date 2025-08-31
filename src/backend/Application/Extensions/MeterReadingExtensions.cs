using Application.DTOs;
using Domain.Entities;

namespace Application.Extensions;

public static class MeterReadingExtensions
{
    public static MeterReadingDto ToDto(this MeterReading meterReading)
    {
        return new MeterReadingDto
        {
            AccountId = meterReading.AccountId,
            MeterReadingDateTime = meterReading.MeterReadingDateTime,
            MeterReadValue = meterReading.MeterReadValue
        };
    }
}