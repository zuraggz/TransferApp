namespace MoneyTransfer.Api.Dtos;

/// <summary>
/// Read-only representation of a transaction record returned to API clients.
/// </summary>
public record TransactionDto(
    int Id,
    int FromAccountId,
    int ToAccountId,
    decimal Amount,
    DateTime Timestamp,
    string Status,
    string? Reason);
