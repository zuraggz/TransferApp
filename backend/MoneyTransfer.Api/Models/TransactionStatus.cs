namespace MoneyTransfer.Api.Models;

/// <summary>
/// Outcome of a transfer attempt. Persisted as a string column so the
/// database remains human-readable and query-friendly.
/// </summary>
public enum TransactionStatus
{
    Success,
    Failed
}
