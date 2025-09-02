using Application.DTOs;
using Application.Extensions;
using Application.Interfaces;
using Application.Services;
using AutoFixture;
using Domain.Entities;
using Domain.Repositories;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Text;

namespace Application.UnitTests.Services;

public class MeterReadingServiceTests
{
    private readonly Mock<IMeterReadingRepository> _mockRepository;
    private readonly Mock<ICsvReader<CsvMeterReadingRowDto>> _mockCsvReader;
    private readonly Mock<IBatchMeterReadingValidator> _mockBatchValidator;
    private readonly MeterReadingService _service;
    private readonly IFixture _fixture;

    public MeterReadingServiceTests()
    {
        _mockRepository = new Mock<IMeterReadingRepository>();
        _mockCsvReader = new Mock<ICsvReader<CsvMeterReadingRowDto>>();
        _mockBatchValidator = new Mock<IBatchMeterReadingValidator>();
        _service = new MeterReadingService(_mockRepository.Object, _mockCsvReader.Object, _mockBatchValidator.Object);
        _fixture = new Fixture();
    }

    [Fact]
    public async Task GetAllMeterReadingsAsync_ReturnsAllReadings()
    {
        // Arrange
        var meterReadings = new List<MeterReading>
        {
            new() { AccountId = 1, MeterReadingDateTime = new DateTime(2024, 1, 1), MeterReadValue = 100 },
            new() { AccountId = 2, MeterReadingDateTime = new DateTime(2024, 1, 2), MeterReadValue = 200 },
            new() { AccountId = 3, MeterReadingDateTime = new DateTime(2024, 1, 3), MeterReadValue = 300 }
        };
        _mockRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(meterReadings);

        // Act
        var result = await _service.GetAllMeterReadingsAsync();

        // Assert
        result.Should().HaveCount(3);
        result.Should().BeEquivalentTo(meterReadings.Select(mr => mr.ToDto()));
        _mockRepository.Verify(x => x.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllMeterReadingsAsync_WithEmptyRepository_ReturnsEmptyCollection()
    {
        // Arrange
        _mockRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<MeterReading>());

        // Act
        var result = await _service.GetAllMeterReadingsAsync();

        // Assert
        result.Should().BeEmpty();
        _mockRepository.Verify(x => x.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task ProcessCsvMeterReadingsAsync_WithValidData_ReturnsSuccessResults()
    {
        // Arrange
        var csvFile = CreateMockCsvFile("test");
        var csvRecord = new CsvMeterReadingRowDto
        {
            AccountId = 1,
            MeterReadingDateTime = new DateTime(2024, 1, 1),
            MeterReadValue = 100,
            RowNumber = 2
        };

        _mockCsvReader.Setup(x => x.ReadAsync(It.IsAny<Stream>()))
            .ReturnsAsync(new CsvReadResult<CsvMeterReadingRowDto>
            {
                Records = { csvRecord }
            });

        _mockBatchValidator.Setup(x => x.ValidateAsync(It.IsAny<List<CsvMeterReadingRowDto>>()))
            .ReturnsAsync(new Dictionary<CsvMeterReadingRowDto, ValidationResult>
            {
                { csvRecord, new ValidationResult() }
            });

        var savedEntity = new MeterReading
        {
            AccountId = 1,
            MeterReadingDateTime = new DateTime(2024, 1, 1),
            MeterReadValue = 100
        };

        _mockRepository.Setup(x => x.AddRangeAsync(It.IsAny<List<MeterReading>>()))
            .ReturnsAsync(new List<MeterReading> { savedEntity });

        // Act
        var result = await _service.ProcessCsvMeterReadingsAsync(csvFile);

        // Assert
        result.TotalProcessed.Should().Be(1);
        result.Successful.Should().Be(1);
        result.Failed.Should().Be(0);
        result.SuccessfulReadings.Should().HaveCount(1);
        _mockBatchValidator.Verify(x => x.ValidateAsync(It.IsAny<List<CsvMeterReadingRowDto>>()), Times.Once);
        _mockRepository.Verify(x => x.AddRangeAsync(It.IsAny<List<MeterReading>>()), Times.Once);
    }

    [Fact]
    public async Task ProcessCsvMeterReadingsAsync_WithValidationErrors_ReturnsErrorResults()
    {
        // Arrange
        var csvFile = CreateMockCsvFile("test");
        var csvRecord = new CsvMeterReadingRowDto
        {
            AccountId = 1,
            MeterReadingDateTime = new DateTime(2024, 1, 1),
            MeterReadValue = 100,
            RowNumber = 2
        };

        _mockCsvReader.Setup(x => x.ReadAsync(It.IsAny<Stream>()))
            .ReturnsAsync(new CsvReadResult<CsvMeterReadingRowDto>
            {
                Records = { csvRecord }
            });

        var validationFailures = new List<ValidationFailure>
        {
            new ValidationFailure("AccountId", "Account not found")
        };
        _mockBatchValidator.Setup(x => x.ValidateAsync(It.IsAny<List<CsvMeterReadingRowDto>>()))
            .ReturnsAsync(new Dictionary<CsvMeterReadingRowDto, ValidationResult>
            {
                { csvRecord, new ValidationResult(validationFailures) }
            });

        // Act
        var result = await _service.ProcessCsvMeterReadingsAsync(csvFile);

        // Assert
        result.TotalProcessed.Should().Be(1);
        result.Successful.Should().Be(0);
        result.Failed.Should().Be(1);
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Error.Should().Be("Account not found");
        result.Errors[0].Row.Should().Be(2);
        _mockRepository.Verify(x => x.AddRangeAsync(It.IsAny<List<MeterReading>>()), Times.Never);
    }

    [Fact]
    public async Task ProcessCsvMeterReadingsAsync_WithCsvParsingErrors_ReturnsParsingErrors()
    {
        // Arrange
        var csvFile = CreateMockCsvFile("test");
        var csvError = new CsvParsingError
        {
            Row = 1,
            Field = "AccountId",
            ErrorMessage = "Invalid format",
            RawData = "invalid,data,row"
        };

        _mockCsvReader.Setup(x => x.ReadAsync(It.IsAny<Stream>()))
            .ReturnsAsync(new CsvReadResult<CsvMeterReadingRowDto>
            {
                Errors = { csvError }
            });

        // Act
        var result = await _service.ProcessCsvMeterReadingsAsync(csvFile);

        // Assert
        result.Failed.Should().Be(1);
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Error.Should().Be("AccountId: Invalid format");
        result.Errors[0].Row.Should().Be(1);
        _mockBatchValidator.Verify(x => x.ValidateAsync(It.IsAny<List<CsvMeterReadingRowDto>>()), Times.Never);
    }

    private static IFormFile CreateMockCsvFile(string content)
    {
        var mockFile = new Mock<IFormFile>();
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

        mockFile.Setup(f => f.OpenReadStream()).Returns(stream);
        mockFile.Setup(f => f.FileName).Returns("test.csv");
        mockFile.Setup(f => f.Length).Returns(content.Length);

        return mockFile.Object;
    }
}