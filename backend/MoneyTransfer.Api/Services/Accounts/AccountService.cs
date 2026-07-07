using Microsoft.EntityFrameworkCore;
using MoneyTransfer.Api.Data;
using MoneyTransfer.Api.Dtos;

namespace MoneyTransfer.Api.Services.Accounts;

/// <inheritdoc cref="IAccountService"/>
public class AccountService : IAccountService
{
    private readonly ApplicationDbContext _dbContext;

    public AccountService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<AccountDto>> GetAllAccountsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Accounts
            .AsNoTracking()
            .OrderBy(a => a.Id)
            .Select(a => new AccountDto(a.Id, a.AccountName, a.Balance))
            .ToListAsync(cancellationToken);
    }
}
