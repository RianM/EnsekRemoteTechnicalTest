using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class MeterReadingRepository(ApplicationDbContext context)
    : Repository<MeterReading>(context), IMeterReadingRepository
{
}