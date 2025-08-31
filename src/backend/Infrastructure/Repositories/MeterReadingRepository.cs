using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class MeterReadingRepository(ApplicationDbContext context)
    : Repository<MeterReading>(context), IMeterReadingRepository
{
    public async Task<MeterReading?> GetLatestByAccountIdAsync(int accountId)
    {
        return await _dbSet
            .Where(mr => mr.AccountId == accountId)
            .OrderByDescending(mr => mr.MeterReadingDateTime)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> ExistsByAccountIdAndDateTimeAndValueAsync(int accountId, DateTime dateTime,
        int meterReadValue)
    {
        return await _dbSet
            .AnyAsync(mr => mr.AccountId == accountId &&
                            mr.MeterReadingDateTime == dateTime &&
                            mr.MeterReadValue == meterReadValue);
    }
}