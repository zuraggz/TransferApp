using MoneyTransfer.Api.Models;

namespace MoneyTransfer.Api.Services.Transfers;

/// <summary>
/// Outcome of a transfer attempt. Used instead of throwing exceptions for
/// expected business failures (insufficient funds, unknown account, etc.),
/// keeping control flow explicit for callers.
/// </summary>
public class TransferResult
{
    public bool Succeeded { get; private init; }
    public Transaction? Transaction { get; private init; }
    public decimal? FromAccountBalance { get; private init; }
    public decimal? ToAccountBalance { get; private init; }
    public string? ErrorMessage { get; private init; }
    public TransferErrorType? ErrorType { get; private init; }

    public static TransferResult Success(Transaction transaction, decimal fromAccountBalance, decimal toAccountBalance) => new()
    {
        Succeeded = true,
        Transaction = transaction,
        FromAccountBalance = fromAccountBalance,
        ToAccountBalance = toAccountBalance
    };

    public static TransferResult Failure(string errorMessage, TransferErrorType errorType) => new()
    {
        Succeeded = false,
        ErrorMessage = errorMessage,
        ErrorType = errorType
    };
}
