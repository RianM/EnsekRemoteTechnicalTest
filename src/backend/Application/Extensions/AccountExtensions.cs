using Application.DTOs;
using Domain.Entities;

namespace Application.Extensions;

public static class AccountExtensions
{
    public static AccountDto ToDto(this Account account)
    {
        return new AccountDto(
            account.AccountId,
            account.FirstName,
            account.LastName
        );
    }
}