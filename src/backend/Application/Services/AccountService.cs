using Application.DTOs;
using Application.Extensions;
using Application.Interfaces;
using Domain.Repositories;

namespace Application.Services;

public class AccountService(IAccountRepository accountRepository) : IAccountService
{
    public async Task<IEnumerable<AccountDto>> GetAllAccountsAsync()
    {
        var accounts = await accountRepository.GetAllAsync();
        return accounts.Select(account => account.ToDto());
    }
}