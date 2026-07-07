using Microsoft.AspNetCore.Mvc;
using MoneyTransfer.Api.Dtos;
using MoneyTransfer.Api.Services.Transfers;

namespace MoneyTransfer.Api.Controllers;

[ApiController]
[Route("api/transfer")]
public class TransferController : ControllerBase
{
    private readonly ITransferService _transferService;

    public TransferController(ITransferService transferService)
    {
        _transferService = transferService;
    }

    /// <summary>
    /// Transfers money from one account to another. The sender must have
    /// sufficient balance; the operation either fully applies or fully fails.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(TransferResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TransferResponseDto>> Transfer(
        [FromBody] TransferRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var result = await _transferService.ExecuteTransferAsync(request, cancellationToken);

        if (!result.Succeeded)
        {
            return result.ErrorType switch
            {
                TransferErrorType.AccountNotFound => Problem(result.ErrorMessage, statusCode: StatusCodes.Status404NotFound),
                TransferErrorType.ConcurrencyConflict => Problem(result.ErrorMessage, statusCode: StatusCodes.Status409Conflict),
                _ => Problem(result.ErrorMessage, statusCode: StatusCodes.Status400BadRequest)
            };
        }

        var transaction = result.Transaction!;
        var response = new TransferResponseDto(
            transaction.Id,
            transaction.FromAccountId,
            transaction.ToAccountId,
            transaction.Amount,
            result.FromAccountBalance!.Value,
            result.ToAccountBalance!.Value,
            transaction.Timestamp);

        return Ok(response);
    }
}
