using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Configuration;

namespace Infrastructure.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Account> Accounts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new AccountConfiguration());

        modelBuilder.Entity<Account>().HasData(SeedData.Accounts);
    }
}