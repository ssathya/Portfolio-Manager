using ApplicationModels.Views;
using Microsoft.AspNetCore.Mvc;
using PsqlAccess;

namespace PresentationHelper.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SecurityWithPScoresController : ControllerBase
{
    #region Private Fields

    private readonly ILogger<SecurityWithPScoresController> logger;
    private readonly IRepository<SecurityWithPScore> secScoreRepository;

    #endregion Private Fields

    #region Public Constructors

    public SecurityWithPScoresController(ILogger<SecurityWithPScoresController> logger
        , IRepository<SecurityWithPScore> secScoreRepository)
    {
        this.logger = logger;
        this.secScoreRepository = secScoreRepository;
    }

    #endregion Public Constructors

    #region Public Methods

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        IEnumerable<SecurityWithPScore> returnValue = await secScoreRepository.FindAll();
        if (!returnValue.Any())
        {
            logger.LogInformation("Did not find any records in database");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
        foreach (var swps in returnValue)
        {
            FixNullValues(swps);
        }
        return Ok(returnValue);
    }

    [HttpGet("{ticker}")]
    public async Task<IActionResult> Get(string ticker)
    {
        SecurityWithPScore? returnValue = (await secScoreRepository.FindAll(r => r.Ticker == ticker.ToUpper().Trim()))
            .FirstOrDefault();
        if (returnValue == null)
        {
            logger.LogInformation($"{ticker.ToUpper().Trim()} not found in database");
            return NotFound();
        }
        FixNullValues(returnValue);
        return Ok(returnValue);
    }

    #endregion Public Methods

    #region Private Methods

    private static void FixNullValues(SecurityWithPScore swps)
    {
        swps.PiotroskiComputedValue = swps.PiotroskiComputedValue ?? 0;
        swps.SimFinRating = swps.SimFinRating ?? 0;
    }

    #endregion Private Methods
}