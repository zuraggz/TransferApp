using Microsoft.EntityFrameworkCore;
using MoneyTransfer.Api.Data;
using MoneyTransfer.Api.Dtos;

namespace MoneyTransfer.Api.Services.Transactions;

/// <inheritdoc cref="ITransactionService"/>
public class TransactionService : ITransactionService
{
    private readonly ApplicationDbContext _dbContext;

    public TransactionService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<TransactionDto>> GetAllTransactionsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Transactions
            .AsNoTracking()
            .OrderByDescending(t => t.Timestamp)
            .Select(t => new TransactionDto(
                t.Id,
                t.FromAccountId,
                t.ToAccountId,
                t.Amount,
                t.Timestamp,
                t.Status.ToString(),
                t.Reason))
            .ToListAsync(cancellationToken);
    }
}
