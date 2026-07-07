namespace MoneyTransfer.Api.Dtos;

/// <summary>
/// Read-only representation of an account returned to API clients.
/// </summary>
public record AccountDto(int Id, string AccountName, decimal Balance);
