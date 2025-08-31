using Domain.Entities;

namespace Domain.Repositories;

public interface IMeterReadingRepository : IRepository<MeterReading>
{
    Task<MeterReading?> GetLatestByAccountIdAsync(int accountId);
    Task<bool> ExistsByAccountIdAndDateTimeAndValueAsync(int accountId, DateTime dateTime, int meterReadValue);
}