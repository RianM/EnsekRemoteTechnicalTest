using Application.DTOs;
using Application.Interfaces;
using Application.Services;
using AutoFixture;
using Domain.Constants;
using Domain.Entities;
using Domain.Repositories;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Text;

namespace Application.UnitTests.Services;

public class MeterReadingServiceTests
{
    private readonly Mock<IMeterReadingRepository> _mockRepository;
    private readonly Mock<IMeterReadingValidator> _mockValidator;
    private readonly MeterReadingService _service;
    private readonly IFixture _fixture;

    private const string ValidHeaders = "AccountId,MeterReadingDateTime,MeterReadValue";
    private static readonly DateTime TestDate1 = new DateTime(2024, 1, 1, 10, 0, 0);
    private static readonly DateTime TestDate2 = new DateTime(2024, 1, 2, 11, 0, 0);
    private const int TestAccount1 = 123;
    private const int TestAccount2 = 456;
    private const int TestValue1 = 12345;
    private const int TestValue2 = 67890;

    public MeterReadingServiceTests()
    {
        _mockRepository = new Mock<IMeterReadingRepository>();
        _mockValidator = new Mock<IMeterReadingValidator>();
        _service = new MeterReadingService(_mockRepository.Object, _mockValidator.Object);
        _fixture = new Fixture();
    }

    #region Header Validation Tests

    [Fact]
    public async Task ProcessCsvMeterReadingsAsync_WithEmptyFile_ReturnsErrorResult()
    {
        // Arrange
        var csvFile = CreateMockCsvFile("");

        // Act
        var result = await _service.ProcessCsvMeterReadingsAsync(csvFile);

        // Assert
        AssertErrorResult(result, 0, 0, 1);
        AssertErrorDetails(result.Errors[0], 1, ErrorMessages.CsvProcessing.EmptyFile, string.Empty);
    }

    [Fact]
    public async Task ProcessCsvMeterReadingsAsync_WithInvalidHeaders_ReturnsErrorResult()
    {
        // Arrange
        var invalidHeaders = "WrongHeader1,WrongHeader2,WrongHeader3";
        var csvFile = CreateMockCsvFile(invalidHeaders);

        // Act
        var result = await _service.ProcessCsvMeterReadingsAsync(csvFile);

        // Assert
        AssertErrorResult(result, 0, 0, 1);
        result.Errors[0].Row.Should().Be(1);
        result.Errors[0].Error.Should().Contain(ErrorMessages.CsvProcessing.InvalidHeaders);
        result.Errors[0].Error.Should().Contain("Expected: AccountId, MeterReadingDateTime, MeterReadValue");
        result.Errors[0].Error.Should().Contain("Found: WrongHeader1, WrongHeader2, WrongHeader3");
        result.Errors[0].RawData.Should().Be(invalidHeaders);
    }

    [Fact]
    public async Task ProcessCsvMeterReadingsAsync_WithWrongNumberOfHeaders_ReturnsErrorResult()
    {
        // Arrange
        var invalidHeaders = "AccountId,MeterReadingDateTime";
        var csvFile = CreateMockCsvFile(invalidHeaders);

        // Act
        var result = await _service.ProcessCsvMeterReadingsAsync(csvFile);

        // Assert
        AssertErrorResult(result, 0, 0, 1);
        result.Errors[0].Row.Should().Be(1);
        result.Errors[0].Error.Should().Contain(ErrorMessages.CsvProcessing.InvalidHeaders);
    }

    [Fact]
    public async Task ProcessCsvMeterReadingsAsync_WithCaseInsensitiveValidHeaders_ContinuesToProcessing()
    {
        // Arrange
        var validHeaders = "accountid,meterreadingdatetime,meterreadvalue";
        var csvContent = validHeaders + "\n";
        var csvFile = CreateMockCsvFile(csvContent);

        // Act
        var result = await _service.ProcessCsvMeterReadingsAsync(csvFile);

        // Assert
        result.Should().NotBeNull();
        result.Errors.Should().BeEmpty();
    }

    #endregion

    #region Data Processing Tests

    [Fact]
    public async Task ProcessCsvMeterReadingsAsync_WithValidRows_ProcessesSuccessfully()
    {
        // Arrange
        var csvContent = CreateCsvContent("123,01/01/2024 10:00,12345", "456,02/01/2024 11:00,67890");
        var csvFile = CreateMockCsvFile(csvContent);

        var csvRow1 = CreateValidCsvRow(TestAccount1, TestDate1, TestValue1, 1);
        var csvRow2 = CreateValidCsvRow(TestAccount2, TestDate2, TestValue2, 2);

        SetupValidParsing("123,01/01/2024 10:00,12345", 1, csvRow1);
        SetupValidParsing("456,02/01/2024 11:00,67890", 2, csvRow2);
        SetupValidValidation(csvRow1, 1);
        SetupValidValidation(csvRow2, 2);
        SetupRepositorySuccess();

        // Act
        var result = await _service.ProcessCsvMeterReadingsAsync(csvFile);

        // Assert
        AssertSuccessfulResult(result, 2, 2, 2);
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<MeterReading>()), Times.Exactly(2));
    }

    [Fact]
    public async Task ProcessCsvMeterReadingsAsync_WithMixedValidInvalidRows_ProcessesPartially()
    {
        // Arrange
        var csvContent = CreateCsvContent("123,01/01/2024 10:00,12345", "invalid,data,row", "456,02/01/2024 11:00,67890");
        var csvFile = CreateMockCsvFile(csvContent);

        var validCsvRow = CreateValidCsvRow(TestAccount1, TestDate1, TestValue1, 1);
        var validCsvRow2 = CreateValidCsvRow(TestAccount2, TestDate2, TestValue2, 3);
        var parseError = CreateErrorDto(2, "Invalid data format", "invalid,data,row");
        var validationError = CreateErrorDto(3, "Account not found", "456,02/01/2024 11:00,67890", TestAccount2);

        SetupValidParsing("123,01/01/2024 10:00,12345", 1, validCsvRow);
        SetupInvalidParsing("invalid,data,row", 2, parseError);
        SetupValidParsing("456,02/01/2024 11:00,67890", 3, validCsvRow2);
        SetupValidValidation(validCsvRow, 1);
        SetupInvalidValidation(validCsvRow2, 3, validationError);
        SetupRepositorySuccess();

        // Act
        var result = await _service.ProcessCsvMeterReadingsAsync(csvFile);

        // Assert
        result.Should().NotBeNull();
        result.TotalProcessed.Should().Be(3);
        result.Successful.Should().Be(1);
        result.Failed.Should().Be(2);
        result.Errors.Should().HaveCount(2);
        result.Errors.Should().Contain(e => e.Row == 2 && e.Error == "Invalid data format");
        result.Errors.Should().Contain(e => e.Row == 3 && e.Error == "Account not found");
        result.SuccessfulReadings.Should().HaveCount(1);

        _mockRepository.Verify(r => r.AddAsync(It.IsAny<MeterReading>()), Times.Once);
    }

    #endregion

    #region Exception Handling Tests

    [Fact]
    public async Task ProcessCsvMeterReadingsAsync_WithFileProcessingException_ReturnsErrorResult()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.OpenReadStream()).Throws(new IOException("File access denied"));

        // Act
        var result = await _service.ProcessCsvMeterReadingsAsync(mockFile.Object);

        // Assert
        AssertErrorResult(result, 0, 0, 1);
        result.Errors[0].Row.Should().Be(0);
        result.Errors[0].Error.Should().Contain(ErrorMessages.CsvProcessing.FileProcessingError);
        result.Errors[0].Error.Should().Contain("File access denied");
        result.Errors[0].RawData.Should().Be(string.Empty);
    }

    [Fact]
    public async Task ProcessCsvMeterReadingsAsync_WithDatabaseException_HandlesErrorGracefully()
    {
        // Arrange
        var csvContent = CreateCsvContent("123,01/01/2024 10:00,12345");
        var csvFile = CreateMockCsvFile(csvContent);

        var validCsvRow = CreateValidCsvRow(TestAccount1, TestDate1, TestValue1, 1);

        SetupValidParsing("123,01/01/2024 10:00,12345", 1, validCsvRow);
        SetupValidValidation(validCsvRow, 1);
        SetupRepositoryFailure(new InvalidOperationException("Database connection failed"));

        // Act
        var result = await _service.ProcessCsvMeterReadingsAsync(csvFile);

        // Assert
        AssertErrorResult(result, 1, 1, 1);
        result.Errors[0].Row.Should().Be(1);
        result.Errors[0].AccountId.Should().Be(TestAccount1);
        result.Errors[0].Error.Should().Contain("Database error: Database connection failed");
        result.Errors[0].RawData.Should().Be("123,01/01/2024 10:00,12345");
    }

    #endregion

    #region Helper Methods

    private static IFormFile CreateMockCsvFile(string content)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        var stream = new MemoryStream(bytes);
        
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.OpenReadStream()).Returns(stream);
        mockFile.Setup(f => f.FileName).Returns("test.csv");
        mockFile.Setup(f => f.Length).Returns(bytes.Length);
        
        return mockFile.Object;
    }

    private CsvMeterReadingRowDto CreateValidCsvRow(int accountId, DateTime dateTime, int meterValue, int rowNumber)
    {
        return _fixture.Build<CsvMeterReadingRowDto>()
            .With(x => x.AccountId, accountId)
            .With(x => x.MeterReadingDateTime, dateTime)
            .With(x => x.MeterReadValue, meterValue)
            .With(x => x.RowNumber, rowNumber)
            .Create();
    }

    private static MeterReadingUploadErrorDto CreateErrorDto(int row, string error, string rawData, int? accountId = null)
    {
        return new MeterReadingUploadErrorDto
        {
            Row = row,
            Error = error,
            RawData = rawData,
            AccountId = accountId
        };
    }

    private static string CreateCsvContent(params string[] dataRows)
    {
        return ValidHeaders + "\n" + string.Join("\n", dataRows);
    }

    private void SetupValidParsing(string csvData, int rowNumber, CsvMeterReadingRowDto expectedDto)
    {
        _mockValidator.Setup(v => v.ParseCsvRow(csvData, rowNumber))
            .Returns((expectedDto, false, new List<MeterReadingUploadErrorDto>()));
    }

    private void SetupInvalidParsing(string csvData, int rowNumber, MeterReadingUploadErrorDto error)
    {
        _mockValidator.Setup(v => v.ParseCsvRow(csvData, rowNumber))
            .Returns((null, true, new List<MeterReadingUploadErrorDto> { error }));
    }

    private void SetupValidValidation(CsvMeterReadingRowDto dto, int rowNumber)
    {
        _mockValidator.Setup(v => v.ValidateMeterReadingRowAsync(dto, rowNumber))
            .ReturnsAsync((false, new List<MeterReadingUploadErrorDto>()));
    }

    private void SetupInvalidValidation(CsvMeterReadingRowDto dto, int rowNumber, MeterReadingUploadErrorDto error)
    {
        _mockValidator.Setup(v => v.ValidateMeterReadingRowAsync(dto, rowNumber))
            .ReturnsAsync((true, new List<MeterReadingUploadErrorDto> { error }));
    }

    private void SetupRepositorySuccess()
    {
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<MeterReading>()))
            .ReturnsAsync((MeterReading mr) => mr);
    }

    private void SetupRepositoryFailure(Exception exception)
    {
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<MeterReading>()))
            .ThrowsAsync(exception);
    }

    private static void AssertSuccessfulResult(MeterReadingUploadResultDto result, int totalProcessed, int successful, int expectedReadings)
    {
        result.Should().NotBeNull();
        result.TotalProcessed.Should().Be(totalProcessed);
        result.Successful.Should().Be(successful);
        result.Failed.Should().Be(totalProcessed - successful);
        result.Errors.Should().BeEmpty();
        result.SuccessfulReadings.Should().HaveCount(expectedReadings);
    }

    private static void AssertErrorResult(MeterReadingUploadResultDto result, int totalProcessed, int failed, int errorCount)
    {
        result.Should().NotBeNull();
        result.TotalProcessed.Should().Be(totalProcessed);
        result.Failed.Should().Be(failed);
        result.Errors.Should().HaveCount(errorCount);
    }

    private static void AssertErrorDetails(MeterReadingUploadErrorDto error, int expectedRow, string expectedMessage, string expectedRawData, int? expectedAccountId = null)
    {
        error.Row.Should().Be(expectedRow);
        error.Error.Should().Be(expectedMessage);
        error.RawData.Should().Be(expectedRawData);
        if (expectedAccountId.HasValue)
            error.AccountId.Should().Be(expectedAccountId.Value);
    }

    #endregion
}