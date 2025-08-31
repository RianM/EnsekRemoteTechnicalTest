using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.UnitTests.Fixtures;

public class InMemoryDbContextFixture : IDisposable
{
    public ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}