using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MoneyTransfer.Api.Models;

/// <summary>
/// A holder of funds that can send and receive money transfers.
/// </summary>
public class Account
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string AccountName { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Balance { get; set; }

    /// <summary>
    /// Concurrency token used by EF Core to detect and reject conflicting
    /// concurrent updates to the same account (e.g. two simultaneous transfers).
    /// </summary>
    [Timestamp]
    public byte[]? RowVersion { get; set; }
}
