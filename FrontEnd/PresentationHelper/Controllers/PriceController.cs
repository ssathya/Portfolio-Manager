using ApplicationModels.Quotes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PsqlAccess;

namespace PresentationHelper.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PriceController : ControllerBase
{
    private readonly ILogger<PriceController> logger;
    private readonly IRepository<YPrice> priceRepository;

    public PriceController(ILogger<PriceController> logger
        , IRepository<YPrice> priceRepository)
    {
        this.logger = logger;
        this.priceRepository = priceRepository;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        IEnumerable<YPrice> returnValue = await priceRepository.FindAll();
        if (!returnValue.Any())
        {
            logger.LogInformation("Did not find any pricing information in database");
            return NotFound();
        }
        return Ok(returnValue);
    }

    [HttpGet("{ticker}")]
    public async Task<IActionResult> Get(string ticker)
    {
        if (string.IsNullOrEmpty(ticker))
        {
            return BadRequest();
        }
        YPrice? priceForSecurity = (await priceRepository.FindAll(r => r.Ticker == ticker.ToUpper().Trim())).FirstOrDefault();
        if (priceForSecurity == null)
        {
            logger.LogInformation($"Price for security {ticker} not in database");
            return NotFound();
        }
        return Ok(priceForSecurity);
    }
}