using Domain.Entities;
using FluentAssertions;
using Infrastructure.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.IntegrationTests.Data;

public class SeedDataTests : IClassFixture<PostgreSqlTestFixture>
{
    private readonly PostgreSqlTestFixture _fixture;

    public SeedDataTests(PostgreSqlTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task SeedData_ShouldPopulateAllAccounts()
    {
        await using var context = _fixture.CreateDbContext();
        
        var accounts = await context.Accounts.ToListAsync();
        
        accounts.Should().HaveCount(27);
    }

    [Fact]
    public async Task SeedData_ShouldContainExpectedAccountData()
    {
        await using var context = _fixture.CreateDbContext();
        
        var firstSeedAccount = await context.Accounts.FirstOrDefaultAsync(a => a.AccountId == 2344);
        var lastSeedAccount = await context.Accounts.FirstOrDefaultAsync(a => a.AccountId == 1248);
        
        firstSeedAccount.Should().NotBeNull();
        firstSeedAccount!.FirstName.Should().Be("Tommy");
        firstSeedAccount.LastName.Should().Be("Test");

        lastSeedAccount.Should().NotBeNull();
        lastSeedAccount!.FirstName.Should().Be("Pam");
        lastSeedAccount.LastName.Should().Be("Test");
    }
}
