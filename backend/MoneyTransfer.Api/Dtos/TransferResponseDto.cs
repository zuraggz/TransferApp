namespace MoneyTransfer.Api.Dtos;

/// <summary>
/// Response body returned after a successful transfer.
/// </summary>
public record TransferResponseDto(
    int TransactionId,
    int FromAccountId,
    int ToAccountId,
    decimal Amount,
    decimal FromAccountBalance,
    decimal ToAccountBalance,
    DateTime Timestamp);
