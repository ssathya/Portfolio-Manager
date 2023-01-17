using ApplicationModels.Compute;
using Microsoft.AspNetCore.Mvc;
using PsqlAccess;

namespace PresentationHelper.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MomMfDolAvgsController : ControllerBase
{
    private readonly ILogger<MomMfDolAvgsController> logger;
    private readonly IRepository<MomMfDolAvg> repository;

    public MomMfDolAvgsController(ILogger<MomMfDolAvgsController> logger
        , IRepository<MomMfDolAvg> repository)
    {
        this.logger = logger;
        this.repository = repository;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var returnValues = await repository.FindAll();
        if (!returnValues.Any())
        {
            logger.LogInformation("Nothing in table MomMfDolAvgs");
            return NotFound();
        }
        return Ok(returnValues);
    }

    [HttpGet("{ticker}")]
    public async Task<IActionResult> Get(string ticker)
    {
        try
        {
            var returnValue = (await repository.FindAll(r => r.Ticker == ticker.ToUpper().Trim()))
                .First();
            if (returnValue == null)
            {
                logger.LogInformation($"{ticker} was not found in table MomMfDolAvgs");
                return NotFound();
            }
            return Ok(returnValue);
        }
        catch (Exception ex)
        {
            logger.LogInformation($"Error occurred while getting values for {ticker}");
            logger.LogInformation($"{ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}