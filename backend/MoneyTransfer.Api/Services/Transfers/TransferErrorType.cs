namespace MoneyTransfer.Api.Services.Transfers;

/// <summary>
/// Categorizes why a transfer failed so the controller can map it to the
/// appropriate HTTP status code without re-deriving business rules.
/// </summary>
public enum TransferErrorType
{
    InvalidAmount,
    SameAccount,
    AccountNotFound,
    InsufficientFunds,
    ConcurrencyConflict
}
