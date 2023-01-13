using ApplicationModels.FinancialStatement.AlphaVantage;
using Microsoft.AspNetCore.Mvc;
using PsqlAccess;

namespace PresentationHelper.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OverviewController : ControllerBase
{
    private readonly ILogger<OverviewController> logger;
    private readonly IRepository<Overview> repository;

    public OverviewController(ILogger<OverviewController> logger
        , IRepository<Overview> repository)
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
        Overview? overview = (await repository.FindAll(r => r.Ticker == ticker.ToUpper().Trim())).FirstOrDefault();
        if (overview == null)
        {
            logger.LogInformation($"Overview for security {ticker} not in database");
            return NotFound();
        }
        return Ok(overview);
    }
}