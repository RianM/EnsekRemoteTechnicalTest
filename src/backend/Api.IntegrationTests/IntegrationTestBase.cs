using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Infrastructure.Data;
using IntegrationTests.Common.Fixtures;

namespace Api.IntegrationTests;

public class IntegrationTestBase : IAsyncLifetime
{
    private readonly PostgreSqlTestFixture _databaseFixture = new();
    private WebApplicationFactory<Program> Factory { get; set; } = null!;
    protected HttpClient HttpClient { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _databaseFixture.InitializeAsync();

        Factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");

                builder.ConfigureServices(services =>
                {
                    RemoveDbContextRegistration(services);
                    AddTestDbContextRegistration(services);
                });
            });

        HttpClient = Factory.CreateClient();
    }

    private static void RemoveDbContextRegistration(IServiceCollection services)
    {
        var descriptor = services.SingleOrDefault(d =>
            d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
        if (descriptor != null)
        {
            services.Remove(descriptor);
        }
    }
    
    private void AddTestDbContextRegistration(IServiceCollection services)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(_databaseFixture.ConnectionString));
    }

    public async Task DisposeAsync()
    {
        await _databaseFixture.DisposeAsync();
        await Factory.DisposeAsync();
        HttpClient.Dispose();
    }
}