using Microsoft.EntityFrameworkCore;
using MoneyTransfer.Api.Data;
using MoneyTransfer.Api.Dtos;
using MoneyTransfer.Api.Models;

namespace MoneyTransfer.Api.Services.Transfers;

/// <inheritdoc cref="ITransferService"/>
public class TransferService : ITransferService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<TransferService> _logger;

    public TransferService(ApplicationDbContext dbContext, ILogger<TransferService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<TransferResult> ExecuteTransferAsync(TransferRequestDto request, CancellationToken cancellationToken = default)
    {
        if (request.Amount <= 0)
        {
            return TransferResult.Failure("Transfer amount must be greater than zero.", TransferErrorType.InvalidAmount);
        }

        if (request.FromAccountId == request.ToAccountId)
        {
            return TransferResult.Failure("Cannot transfer money to the same account.", TransferErrorType.SameAccount);
        }

        // All reads/writes below happen inside one database transaction so a
        // partial update (e.g. debit succeeds but credit fails) is impossible.
        await using var dbTransaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var fromAccount = await _dbContext.Accounts
                .FirstOrDefaultAsync(a => a.Id == request.FromAccountId, cancellationToken);
            if (fromAccount is null)
            {
                return TransferResult.Failure($"Account {request.FromAccountId} was not found.", TransferErrorType.AccountNotFound);
            }

            var toAccount = await _dbContext.Accounts
                .FirstOrDefaultAsync(a => a.Id == request.ToAccountId, cancellationToken);
            if (toAccount is null)
            {
                return TransferResult.Failure($"Account {request.ToAccountId} was not found.", TransferErrorType.AccountNotFound);
            }

            if (fromAccount.Balance < request.Amount)
            {
                // Both accounts exist, so we can log this as a proper audit record
                // instead of just returning an error.
                var failedTransaction = new Transaction
                {
                    FromAccountId = fromAccount.Id,
                    ToAccountId = toAccount.Id,
                    Amount = request.Amount,
                    Timestamp = DateTime.UtcNow,
                    Status = TransactionStatus.Failed,
                    Reason = "Insufficient balance."
                };

                _dbContext.Transactions.Add(failedTransaction);
                await _dbContext.SaveChangesAsync(cancellationToken);
                await dbTransaction.CommitAsync(cancellationToken);

                return TransferResult.Failure("Account has insufficient balance for this transfer.", TransferErrorType.InsufficientFunds);
            }

            fromAccount.Balance -= request.Amount;
            toAccount.Balance += request.Amount;

            var transaction = new Transaction
            {
                FromAccountId = fromAccount.Id,
                ToAccountId = toAccount.Id,
                Amount = request.Amount,
                Timestamp = DateTime.UtcNow,
                Status = TransactionStatus.Success,
                Reason = null
            };
            _dbContext.Transactions.Add(transaction);

            await _dbContext.SaveChangesAsync(cancellationToken);
            await dbTransaction.CommitAsync(cancellationToken);

            return TransferResult.Success(transaction, fromAccount.Balance, toAccount.Balance);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // One of the accounts was modified by another operation between our
            // read and write. Roll back and ask the caller to retry.
            _logger.LogWarning(ex, "Concurrency conflict transferring {Amount} from {FromAccountId} to {ToAccountId}",
                request.Amount, request.FromAccountId, request.ToAccountId);
            await dbTransaction.RollbackAsync(cancellationToken);
            return TransferResult.Failure("The account was updated by another operation. Please retry the transfer.", TransferErrorType.ConcurrencyConflict);
        }
        catch
        {
            await dbTransaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
