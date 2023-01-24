using ApplicationModels.FinancialStatement.AlphaVantage;
using Microsoft.AspNetCore.Mvc;
using PsqlAccess;

namespace PresentationHelper.Controllers;

[Route("api/[controller]")]
[ApiController]
public class IncomeStatementController : ControllerBase
{
    private readonly ILogger<IncomeStatementController> logger;
    private readonly IRepository<IncomeStatement> repository;

    public IncomeStatementController(ILogger<IncomeStatementController> logger
        , IRepository<IncomeStatement> repository)
    {
        this.logger = logger;
        this.repository = repository;
    }

    [HttpGet("{ticker}")]
    public async Task<IActionResult> Get(string ticker)
    {
        if (string.IsNullOrEmpty(ticker))
        {
            return BadRequest();
        }
        IncomeStatement? incomeStatement;
        try
        {
            incomeStatement = (await repository.FindAll(r => r.Ticker == ticker.ToUpper().Trim())).FirstOrDefault();
        }
        catch (Exception ex)
        {
            logger.LogError($"Exception while retrieving data for ticker {ticker}");
            logger.LogError(ex.Message);
            return StatusCode(StatusCodes.Status503ServiceUnavailable);
        }
        if (incomeStatement == null)
        {
            logger.LogInformation($"Income statement for security {ticker} not in database");
            return NotFound();
        }
        return Ok(incomeStatement);
    }
}