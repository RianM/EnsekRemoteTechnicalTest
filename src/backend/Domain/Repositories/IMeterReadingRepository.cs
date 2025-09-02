using Domain.Entities;

namespace Domain.Repositories;

public interface IMeterReadingRepository : IRepository<MeterReading>
{
    Task<MeterReading?> GetLatestByAccountIdAsync(int accountId);
    Task<bool> ExistsByAccountIdAndDateTimeAndValueAsync(int accountId, DateTime dateTime, int meterReadValue);
    Task<IDictionary<int, MeterReading>> GetLatestByAccountIdsAsync(List<int> accountIds);
    Task<List<MeterReading>> GetExistingReadingsAsync(List<(int AccountId, DateTime DateTime, int Value)> readings);
    Task<List<MeterReading>> AddRangeAsync(List<MeterReading> meterReadings);
}