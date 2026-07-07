using MoneyTransfer.Api.Dtos;

namespace MoneyTransfer.Api.Services.Accounts;

/// <summary>
/// Read access to account data.
/// </summary>
public interface IAccountService
{
    Task<IReadOnlyList<AccountDto>> GetAllAccountsAsync(CancellationToken cancellationToken = default);
}
