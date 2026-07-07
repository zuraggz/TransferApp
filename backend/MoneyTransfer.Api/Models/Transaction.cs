using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MoneyTransfer.Api.Models;

/// <summary>
/// An audit record of a single transfer attempt between two accounts,
/// recorded whether the transfer succeeded or failed.
/// </summary>
public class Transaction
{
    public int Id { get; set; }

    public int FromAccountId { get; set; }
    public Account? FromAccount { get; set; }

    public int ToAccountId { get; set; }
    public Account? ToAccount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public TransactionStatus Status { get; set; }

    /// <summary>
    /// Explanation for a failed transfer (e.g. "Insufficient balance").
    /// Null for successful transfers.
    /// </summary>
    [MaxLength(500)]
    public string? Reason { get; set; }
}
