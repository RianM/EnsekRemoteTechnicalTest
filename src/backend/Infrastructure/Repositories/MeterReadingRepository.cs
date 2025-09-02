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

    public async Task<IDictionary<int, MeterReading>> GetLatestByAccountIdsAsync(List<int> accountIds)
    {
        var latestReadings = await _dbSet
            .Where(mr => accountIds.Contains(mr.AccountId))
            .GroupBy(mr => mr.AccountId)
            .Select(g => g.OrderByDescending(mr => mr.MeterReadingDateTime).First())
            .ToListAsync();

        return latestReadings.ToDictionary(mr => mr.AccountId);
    }

    public async Task<List<MeterReading>> GetExistingReadingsAsync(List<(int AccountId, DateTime DateTime, int Value)> readings)
    {
        var existingReadings = new List<MeterReading>();
        
        foreach (var reading in readings)
        {
            var existing = await _dbSet
                .Where(mr => mr.AccountId == reading.AccountId &&
                           mr.MeterReadingDateTime == reading.DateTime &&
                           mr.MeterReadValue == reading.Value)
                .FirstOrDefaultAsync();
                
            if (existing != null)
            {
                existingReadings.Add(existing);
            }
        }
        
        return existingReadings;
    }

    public async Task<List<MeterReading>> AddRangeAsync(List<MeterReading> meterReadings)
    {
        await _dbSet.AddRangeAsync(meterReadings);
        await _context.SaveChangesAsync();
        return meterReadings;
    }
}