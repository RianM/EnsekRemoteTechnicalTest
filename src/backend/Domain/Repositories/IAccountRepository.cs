using Domain.Entities;

namespace Domain.Repositories;

public interface IAccountRepository : IRepository<Account>
{
    Task<List<Account>> GetByIdsAsync(List<int> accountIds);
}