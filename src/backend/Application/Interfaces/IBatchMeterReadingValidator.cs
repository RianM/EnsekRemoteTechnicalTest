using Application.DTOs;
using FluentValidation.Results;

namespace Application.Interfaces;

public interface IBatchMeterReadingValidator
{
    Task<IDictionary<CsvMeterReadingRowDto, ValidationResult>> ValidateAsync(List<CsvMeterReadingRowDto> readings);
}