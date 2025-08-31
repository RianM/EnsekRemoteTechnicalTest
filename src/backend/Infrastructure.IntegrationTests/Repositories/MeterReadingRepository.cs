using Domain.Entities;
using FluentAssertions;
using Infrastructure.Data;
using IntegrationTests.Common.Fixtures;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.IntegrationTests.Repositories;

public class MeterReadingRepositoryTests : IClassFixture<PostgreSqlTestFixture>
{
    private readonly PostgreSqlTestFixture _fixture;

    public MeterReadingRepositoryTests(PostgreSqlTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Theory]
    [InlineData(0)]      // Minimum boundary
    [InlineData(50000)]  // Mid-range value
    [InlineData(99999)]  // Maximum boundary  
    public async Task AddAsync_WithValidValues_ShouldSucceed(int meterReadValue)
    {
        // Arrange
        await using var context = _fixture.CreateDbContext();
        var repository = new MeterReadingRepository(context);
        
        var meterReading = new MeterReading
        {
            AccountId = SeedData.Accounts.First().AccountId,
            MeterReadingDateTime = DateTime.UtcNow,
            MeterReadValue = meterReadValue
        };

        // Act
        var result = await repository.AddAsync(meterReading);

        // Assert
        result.Should().NotBeNull();
        result.MeterReadValue.Should().Be(meterReadValue);
    }

    [Theory]
    [InlineData(-1)]      // Below minimum
    [InlineData(100000)]  // Above maximum
    public async Task AddAsync_WithInvalidValues_ShouldThrowDbUpdateException(int meterReadValue)
    {
        // Arrange
        await using var context = _fixture.CreateDbContext();
        var repository = new MeterReadingRepository(context);
        
        var meterReading = new MeterReading
        {
            AccountId = SeedData.Accounts.First().AccountId,
            MeterReadingDateTime = DateTime.UtcNow,
            MeterReadValue = meterReadValue
        };

        // Act & Assert
        var act = async () => await repository.AddAsync(meterReading);
        await act.Should().ThrowAsync<DbUpdateException>();
    }

}