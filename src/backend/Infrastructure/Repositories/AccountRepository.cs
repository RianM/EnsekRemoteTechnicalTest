using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class AccountRepository(ApplicationDbContext context) : Repository<Account>(context), IAccountRepository
{
    public async Task<List<Account>> GetByIdsAsync(List<int> accountIds)
    {
        return await _context.Set<Account>()
            .Where(a => accountIds.Contains(a.AccountId))
            .ToListAsync();
    }
}