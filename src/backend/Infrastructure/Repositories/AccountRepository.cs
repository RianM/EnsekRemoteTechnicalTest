using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Data;

namespace Infrastructure.Repositories;

public class AccountRepository(ApplicationDbContext context) : Repository<Account>(context), IAccountRepository;