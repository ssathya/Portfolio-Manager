using ApplicationModels.FinancialStatement.AlphaVantage;
using Microsoft.AspNetCore.Mvc;
using PsqlAccess;

namespace PresentationHelper.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CashFlowController : ControllerBase
{
    private readonly ILogger<CashFlowController> logger;
    private readonly IRepository<CashFlow> repository;

    public CashFlowController(ILogger<CashFlowController> logger
        , IRepository<CashFlow> repository)
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
        CashFlow? cashFlow;
        try
        {
            cashFlow = (await repository.FindAll(r => r.Ticker == ticker.ToUpper().Trim())).FirstOrDefault();
        }
        catch (Exception ex)
        {
            logger.LogError($"Exception while retrieving data for ticker {ticker}");
            logger.LogError(ex.Message);
            return StatusCode(StatusCodes.Status503ServiceUnavailable);
        }
        if (cashFlow == null)
        {
            logger.LogInformation($"Cash flow statement for security {ticker} not in database");
            return NotFound();
        }
        return Ok(cashFlow);
    }
}