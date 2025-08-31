using AutoFixture;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Infrastructure.UnitTests.Fixtures;

namespace Infrastructure.UnitTests.Repositories;

public class MeterReadingRepositoryTests : IDisposable
{
    private readonly InMemoryDbContextFixture _fixture;
    private readonly Fixture _autoFixture;
    private readonly ApplicationDbContext _context;

    public MeterReadingRepositoryTests()
    {
        _fixture = new InMemoryDbContextFixture();
        _autoFixture = new Fixture();
        _context = _fixture.CreateDbContext();
    }

    [Fact]
    public async Task GetLatestByAccountIdAsync_WhenNoResults_ShouldReturnNull()
    {
        // Arrange
        var repository = new MeterReadingRepository(_context);
        var accountId = _autoFixture.Create<int>();

        // Act
        var result = await repository.GetLatestByAccountIdAsync(accountId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetLatestByAccountIdAsync_WhenTwoResults_ShouldReturnLatest()
    {
        // Arrange
        var accountId = _autoFixture.Create<int>();
        var baseDateTime = DateTime.UtcNow;
        
        var olderReading = _autoFixture.Build<MeterReading>()
            .With(mr => mr.AccountId, accountId)
            .With(mr => mr.MeterReadingDateTime, baseDateTime.AddDays(-2))
            .Without(mr => mr.Account)
            .Create();
        
        var newerReading = _autoFixture.Build<MeterReading>()
            .With(mr => mr.AccountId, accountId)
            .With(mr => mr.MeterReadingDateTime, baseDateTime.AddDays(-1))
            .Without(mr => mr.Account)
            .Create();
        
        _context.MeterReadings.AddRange(olderReading, newerReading);
        await _context.SaveChangesAsync();
        
        var repository = new MeterReadingRepository(_context);

        // Act
        var result = await repository.GetLatestByAccountIdAsync(accountId);

        // Assert
        result.Should().NotBeNull();
        result!.MeterReadValue.Should().Be(newerReading.MeterReadValue);
        result.MeterReadingDateTime.Should().Be(newerReading.MeterReadingDateTime);
    }

    [Fact]
    public async Task ExistsByAccountIdAndDateTimeAndValueAsync_WhenMatches_ShouldReturnTrue()
    {
        // Arrange
        var existingReading = _autoFixture.Build<MeterReading>()
            .Without(mr => mr.Account)
            .Create();
        
        _context.MeterReadings.Add(existingReading);
        await _context.SaveChangesAsync();
        
        var repository = new MeterReadingRepository(_context);

        // Act
        var result = await repository.ExistsByAccountIdAndDateTimeAndValueAsync(
            existingReading.AccountId, 
            existingReading.MeterReadingDateTime, 
            existingReading.MeterReadValue);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByAccountIdAndDateTimeAndValueAsync_WhenDoesntMatch_ShouldReturnFalse()
    {
        // Arrange
        var existingReading = _autoFixture.Build<MeterReading>()
            .Without(mr => mr.Account)
            .Create();
        var searchCriteria = _autoFixture.Build<MeterReading>()
            .Without(mr => mr.Account)
            .Create();
        
        _context.MeterReadings.Add(existingReading);
        await _context.SaveChangesAsync();
        
        var repository = new MeterReadingRepository(_context);

        // Act
        var result = await repository.ExistsByAccountIdAndDateTimeAndValueAsync(
            searchCriteria.AccountId, 
            searchCriteria.MeterReadingDateTime, 
            searchCriteria.MeterReadValue);

        // Assert
        result.Should().BeFalse();
    }

    public void Dispose()
    {
        _context?.Dispose();
        _fixture?.Dispose();
    }
}