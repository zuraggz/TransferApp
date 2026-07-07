using MoneyTransfer.Api.Dtos;

namespace MoneyTransfer.Api.Services.Transfers;

/// <summary>
/// Executes money transfers between two accounts.
/// </summary>
public interface ITransferService
{
    /// <summary>
    /// Validates and executes a transfer from one account to another as a single
    /// atomic database operation. Neither account's balance will go negative.
    /// </summary>
    Task<TransferResult> ExecuteTransferAsync(TransferRequestDto request, CancellationToken cancellationToken = default);
}
