using Application.DTOs;
using Application.Validators;
using Domain.Constants;
using Domain.Entities;
using Domain.Repositories;
using FluentAssertions;
using Moq;

namespace Application.UnitTests.Validators;

public class MeterReadingValidatorTests
{
    private readonly Mock<IAccountRepository> _mockAccountRepository;
    private readonly Mock<IMeterReadingRepository> _mockMeterReadingRepository;
    private readonly MeterReadingValidator _validator;

    public MeterReadingValidatorTests()
    {
        _mockAccountRepository = new Mock<IAccountRepository>();
        _mockMeterReadingRepository = new Mock<IMeterReadingRepository>();
        _validator = new MeterReadingValidator(_mockAccountRepository.Object, _mockMeterReadingRepository.Object);
    }

    [Fact]
    public void ParseCsvRow_ValidRow_ReturnsSuccessWithData()
    {
        // Arrange
        const string csvLine = "123,22/04/2019 09:24,01002";
        const int rowNumber = 0;

        // Act
        var result = _validator.ParseCsvRow(csvLine, rowNumber);

        // Assert
        result.HasValidationErrors.Should().BeFalse();
        result.Errors.Should().BeEmpty();
        result.Data.Should().NotBeNull();
        result.Data!.AccountId.Should().Be(123);
        result.Data.MeterReadingDateTime.Should().Be(new DateTime(2019, 4, 22, 9, 24, 0, DateTimeKind.Utc));
        result.Data.MeterReadValue.Should().Be(1002);
        result.Data.RowNumber.Should().Be(1);
    }

    [Theory]
    [InlineData("123,22/04/2019 09:24,01002")]
    [InlineData("456,2/04/2019 09:24,01002")]
    [InlineData("789,22/4/2019 09:24,01002")]
    [InlineData("101,2/4/2019 09:24,01002")]
    public void ParseCsvRow_ValidDateTimeFormats_ReturnsSuccess(string csvLine)
    {
        // Arrange
        const int rowNumber = 0;

        // Act
        var result = _validator.ParseCsvRow(csvLine, rowNumber);

        // Assert
        result.HasValidationErrors.Should().BeFalse();
        result.Data.Should().NotBeNull();
    }

    [Fact]
    public void ParseCsvRow_InsufficientColumns_ReturnsError()
    {
        // Arrange
        const string csvLine = "123,22/04/2019 09:24";
        const int rowNumber = 0;

        // Act
        var result = _validator.ParseCsvRow(csvLine, rowNumber);

        // Assert
        result.HasValidationErrors.Should().BeTrue();
        result.Data.Should().BeNull();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Row.Should().Be(1);
        result.Errors[0].Error.Should().Be(ErrorMessages.CsvProcessing.InsufficientColumns);
        result.Errors[0].RawData.Should().Be(csvLine);
    }

    [Fact]
    public void ParseCsvRow_InvalidAccountId_ReturnsError()
    {
        // Arrange
        const string csvLine = "invalid,22/04/2019 09:24,01002";
        const int rowNumber = 0;

        // Act
        var result = _validator.ParseCsvRow(csvLine, rowNumber);

        // Assert
        result.HasValidationErrors.Should().BeTrue();
        result.Data.Should().BeNull();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Row.Should().Be(1);
        result.Errors[0].Error.Should().Be(ErrorMessages.MeterReading.InvalidAccountIdFormat);
        result.Errors[0].RawData.Should().Be(csvLine);
    }

    [Theory]
    [InlineData("123,,01002")]
    [InlineData("123,invalid-date,01002")]
    [InlineData("123,22/13/2019 09:24,01002")]
    public void ParseCsvRow_InvalidDateTime_ReturnsError(string csvLine)
    {
        // Arrange
        const int rowNumber = 0;

        // Act
        var result = _validator.ParseCsvRow(csvLine, rowNumber);

        // Assert
        result.HasValidationErrors.Should().BeTrue();
        result.Data.Should().BeNull();
        result.Errors.Should().ContainSingle(e => e.Error == ErrorMessages.MeterReading.InvalidDateTimeFormat);
    }

    [Fact]
    public void ParseCsvRow_InvalidMeterValue_ReturnsError()
    {
        // Arrange
        const string csvLine = "123,22/04/2019 09:24,invalid";
        const int rowNumber = 0;

        // Act
        var result = _validator.ParseCsvRow(csvLine, rowNumber);

        // Assert
        result.HasValidationErrors.Should().BeTrue();
        result.Data.Should().BeNull();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Row.Should().Be(1);
        result.Errors[0].Error.Should().Be(ErrorMessages.MeterReading.InvalidMeterValueFormat);
        result.Errors[0].RawData.Should().Be(csvLine);
    }

    [Fact]
    public void ParseCsvRow_MultipleErrors_ReturnsAllErrors()
    {
        // Arrange
        const string csvLine = "invalid,,invalid";
        const int rowNumber = 0;

        // Act
        var result = _validator.ParseCsvRow(csvLine, rowNumber);

        // Assert
        result.HasValidationErrors.Should().BeTrue();
        result.Data.Should().BeNull();
        result.Errors.Should().HaveCount(3);
        result.Errors.Should().Contain(e => e.Error == ErrorMessages.MeterReading.InvalidAccountIdFormat);
        result.Errors.Should().Contain(e => e.Error == ErrorMessages.MeterReading.InvalidDateTimeFormat);
        result.Errors.Should().Contain(e => e.Error == ErrorMessages.MeterReading.InvalidMeterValueFormat);
    }

    [Fact]
    public async Task ValidateMeterReadingRowAsync_ValidRow_ReturnsNoErrors()
    {
        // Arrange
        var row = new CsvMeterReadingRowDto
        {
            AccountId = 123,
            MeterReadingDateTime = DateTime.UtcNow,
            MeterReadValue = 50000,
            RowNumber = 1
        };
        var account = new Account { AccountId = 123, FirstName = "John", LastName = "Doe" };

        _mockAccountRepository.Setup(x => x.GetByIdAsync(123))
            .ReturnsAsync(account);
        _mockMeterReadingRepository.Setup(x => x.ExistsByAccountIdAndDateTimeAndValueAsync(123, row.MeterReadingDateTime, 50000))
            .ReturnsAsync(false);
        _mockMeterReadingRepository.Setup(x => x.GetLatestByAccountIdAsync(123))
            .ReturnsAsync((MeterReading?)null);

        // Act
        var result = await _validator.ValidateMeterReadingRowAsync(row, 0);

        // Assert
        result.HasErrors.Should().BeFalse();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(100000)]
    public async Task ValidateMeterReadingRowAsync_ValueOutOfRange_ReturnsError(int meterValue)
    {
        // Arrange
        var row = new CsvMeterReadingRowDto
        {
            AccountId = 123,
            MeterReadingDateTime = DateTime.UtcNow,
            MeterReadValue = meterValue,
            RowNumber = 1
        };
        var account = new Account { AccountId = 123, FirstName = "John", LastName = "Doe" };

        _mockAccountRepository.Setup(x => x.GetByIdAsync(123))
            .ReturnsAsync(account);
        _mockMeterReadingRepository.Setup(x => x.ExistsByAccountIdAndDateTimeAndValueAsync(123, row.MeterReadingDateTime, meterValue))
            .ReturnsAsync(false);
        _mockMeterReadingRepository.Setup(x => x.GetLatestByAccountIdAsync(123))
            .ReturnsAsync((MeterReading?)null);

        // Act
        var result = await _validator.ValidateMeterReadingRowAsync(row, 0);

        // Assert
        result.HasErrors.Should().BeTrue();
        result.Errors.Should().ContainSingle();
        result.Errors[0].Error.Should().Be(ErrorMessages.MeterReading.ValueOutOfRange);
        result.Errors[0].AccountId.Should().Be(123);
    }

    [Fact]
    public async Task ValidateMeterReadingRowAsync_AccountNotFound_ReturnsError()
    {
        // Arrange
        var row = new CsvMeterReadingRowDto
        {
            AccountId = 999,
            MeterReadingDateTime = DateTime.UtcNow,
            MeterReadValue = 50000,
            RowNumber = 1
        };

        _mockAccountRepository.Setup(x => x.GetByIdAsync(999))
            .ReturnsAsync((Account?)null);

        // Act
        var result = await _validator.ValidateMeterReadingRowAsync(row, 0);

        // Assert
        result.HasErrors.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Error == string.Format(ErrorMessages.MeterReading.AccountNotFound, 999));
    }

    [Fact]
    public async Task ValidateMeterReadingRowAsync_DuplicateEntry_ReturnsError()
    {
        // Arrange
        var row = new CsvMeterReadingRowDto
        {
            AccountId = 123,
            MeterReadingDateTime = new DateTime(2019, 4, 22, 9, 24, 0, DateTimeKind.Utc),
            MeterReadValue = 50000,
            RowNumber = 1
        };
        var account = new Account { AccountId = 123, FirstName = "John", LastName = "Doe" };

        _mockAccountRepository.Setup(x => x.GetByIdAsync(123))
            .ReturnsAsync(account);
        _mockMeterReadingRepository.Setup(x => x.ExistsByAccountIdAndDateTimeAndValueAsync(123, row.MeterReadingDateTime, 50000))
            .ReturnsAsync(true);

        // Act
        var result = await _validator.ValidateMeterReadingRowAsync(row, 0);

        // Assert
        result.HasErrors.Should().BeTrue();
        result.Errors.Should().ContainSingle();
        result.Errors[0].Error.Should().Be(ErrorMessages.MeterReading.DuplicateEntry);
        result.Errors[0].AccountId.Should().Be(123);
    }

    [Fact]
    public async Task ValidateMeterReadingRowAsync_ReadingTooOld_ReturnsError()
    {
        // Arrange
        var oldDateTime = new DateTime(2019, 4, 22, 9, 24, 0, DateTimeKind.Utc);
        var newDateTime = new DateTime(2019, 4, 20, 9, 24, 0, DateTimeKind.Utc);
        
        var row = new CsvMeterReadingRowDto
        {
            AccountId = 123,
            MeterReadingDateTime = newDateTime,
            MeterReadValue = 50000,
            RowNumber = 1
        };
        var account = new Account { AccountId = 123, FirstName = "John", LastName = "Doe" };
        var latestReading = new MeterReading 
        { 
            AccountId = 123, 
            MeterReadingDateTime = oldDateTime,
            MeterReadValue = 40000
        };

        _mockAccountRepository.Setup(x => x.GetByIdAsync(123))
            .ReturnsAsync(account);
        _mockMeterReadingRepository.Setup(x => x.ExistsByAccountIdAndDateTimeAndValueAsync(123, row.MeterReadingDateTime, 50000))
            .ReturnsAsync(false);
        _mockMeterReadingRepository.Setup(x => x.GetLatestByAccountIdAsync(123))
            .ReturnsAsync(latestReading);

        // Act
        var result = await _validator.ValidateMeterReadingRowAsync(row, 0);

        // Assert
        result.HasErrors.Should().BeTrue();
        result.Errors.Should().ContainSingle();
        result.Errors[0].Error.Should().Be(string.Format(ErrorMessages.MeterReading.ReadingTooOld, 
            newDateTime.ToString("dd/MM/yyyy HH:mm"), 
            oldDateTime.ToString("dd/MM/yyyy HH:mm")));
        result.Errors[0].AccountId.Should().Be(123);
    }
}