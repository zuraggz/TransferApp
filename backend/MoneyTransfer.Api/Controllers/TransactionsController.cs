using Microsoft.AspNetCore.Mvc;
using MoneyTransfer.Api.Dtos;
using MoneyTransfer.Api.Services.Transactions;

namespace MoneyTransfer.Api.Controllers;

[ApiController]
[Route("api/transactions")]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _transactionService;

    public TransactionsController(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    /// <summary>
    /// Returns all transactions (both successful and failed), most recent first.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TransactionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TransactionDto>>> GetTransactions(CancellationToken cancellationToken)
    {
        var transactions = await _transactionService.GetAllTransactionsAsync(cancellationToken);
        return Ok(transactions);
    }
}
