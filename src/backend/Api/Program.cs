using Application.Interfaces;
using Application.Services;
using Domain.Repositories;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repository Registration
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IMeterReadingRepository, MeterReadingRepository>();

// Service Registration
builder.Services.AddScoped<IMeterReadingService, MeterReadingService>();

// Validator Registration
builder.Services.AddScoped<IMeterReadingValidator, Application.Validators.MeterReadingValidator>();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

// Make Program class accessible for testing
public partial class Program { }
