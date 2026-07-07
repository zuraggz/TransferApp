using MoneyTransfer.Api.Dtos;

namespace MoneyTransfer.Api.Services.Transactions;

/// <summary>
/// Read access to transaction history.
/// </summary>
public interface ITransactionService
{
    Task<IReadOnlyList<TransactionDto>> GetAllTransactionsAsync(CancellationToken cancellationToken = default);
}
