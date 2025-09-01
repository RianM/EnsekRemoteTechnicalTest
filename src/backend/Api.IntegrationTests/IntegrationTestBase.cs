using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Infrastructure.Data;
using IntegrationTests.Common.Fixtures;
using System.Text;
using System.Text.Json;

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

    protected async Task<string> GetManagerTokenAsync()
    {
        var response = await HttpClient.PostAsync("/api/auth/token/manager", null);
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        
        return tokenResponse!.Token;
    }

    protected async Task<string> GetAnonymousTokenAsync()
    {
        var response = await HttpClient.PostAsync("/api/auth/token/anonymous", null);
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        
        return tokenResponse!.Token;
    }

    protected void SetAuthorizationHeader(string token)
    {
        HttpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    private class TokenResponse
    {
        public string Token { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}