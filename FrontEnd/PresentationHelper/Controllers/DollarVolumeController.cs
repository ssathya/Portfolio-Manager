using ApplicationModels.Quotes;
using Microsoft.AspNetCore.Mvc;
using PsqlAccess;

namespace PresentationHelper.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DollarVolumeController : ControllerBase
{
    private readonly ILogger<DollarVolumeController> logger;
    private readonly IRepository<YPrice> priceRepository;
    private const int daysToCompute = 20;

    public DollarVolumeController(ILogger<DollarVolumeController> logger
        , IRepository<YPrice> priceRepository)
    {
        this.logger = logger;
        this.priceRepository = priceRepository;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        IEnumerable<YPrice> prices = await priceRepository.FindAll();
        if (!prices.Any())
        {
            logger.LogInformation("Did not find any pricing information in database");
            return NotFound();
        }
        List<DollarVolume> dollarVolumes = new();
        foreach (var priceRcd in prices)
        {
            IEnumerable<decimal> selValues = priceRcd.CompressedQuotes
                .OrderBy(x => x.Date)
                .Select(x => (x.ClosingPrice * x.Volume))
                .TakeLast(daysToCompute);
            dollarVolumes.Add(new DollarVolume
            {
                Ticker = priceRcd.Ticker,
                Value = (selValues.Sum() / daysToCompute)
            });
        }
        return Ok(dollarVolumes);
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
        IEnumerable<decimal> selValues = priceForSecurity.CompressedQuotes
             .OrderBy(x => x.Date)
             .Select(x => (x.ClosingPrice * x.Volume))
             .TakeLast(daysToCompute);
        DollarVolume dollarVolume = new DollarVolume
        {
            Ticker = priceForSecurity.Ticker,
            Value = (selValues.Sum() / daysToCompute)
        };
        return Ok(dollarVolume);
    }
}