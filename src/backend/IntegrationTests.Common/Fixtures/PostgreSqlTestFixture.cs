using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit;

namespace IntegrationTests.Common.Fixtures;

public class PostgreSqlTestFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container;
    
    public PostgreSqlTestFixture()
    {
        _container = new PostgreSqlBuilder()
            .WithDatabase("TestDb")
            .WithUsername("testuser")
            .WithPassword("testpass")
            .Build();
    }

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        
        await using var context = CreateDbContext();
        await context.Database.EnsureCreatedAsync();
    }

    public ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(_container.GetConnectionString())
            .Options;

        return new ApplicationDbContext(options);
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}