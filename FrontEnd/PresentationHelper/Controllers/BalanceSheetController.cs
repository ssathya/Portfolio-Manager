using ApplicationModels.FinancialStatement.AlphaVantage;
using Microsoft.AspNetCore.Mvc;
using PsqlAccess;

namespace PresentationHelper.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BalanceSheetController : ControllerBase
{
    private readonly ILogger<BalanceSheetController> logger;
    private readonly IRepository<BalanceSheet> repository;

    public BalanceSheetController(ILogger<BalanceSheetController> logger
        , IRepository<BalanceSheet> repository)
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
        BalanceSheet? bSheet = (await repository.FindAll(r => r.Ticker == ticker.ToUpper().Trim())).FirstOrDefault();
        if (bSheet == null)
        {
            logger.LogInformation($"Balance sheet for security {ticker} not in database");
            return NotFound();
        }
        return Ok(bSheet);
    }
}