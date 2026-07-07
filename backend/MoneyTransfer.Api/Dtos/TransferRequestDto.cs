using System.ComponentModel.DataAnnotations;

namespace MoneyTransfer.Api.Dtos;

/// <summary>
/// Request body for POST /api/transfer.
/// </summary>
public class TransferRequestDto
{
    [Required]
    public int FromAccountId { get; set; }

    [Required]
    public int ToAccountId { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
    public decimal Amount { get; set; }
}
