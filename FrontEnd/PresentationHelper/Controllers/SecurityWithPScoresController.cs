using ApplicationModels.Views;
using Microsoft.AspNetCore.Mvc;
using PsqlAccess;

namespace PresentationHelper.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SecurityWithPScoresController : ControllerBase
{
    private readonly ILogger<SecurityWithPScoresController> logger;
    private readonly IRepository<SecurityWithPScore> secScoreRepository;

    public SecurityWithPScoresController(ILogger<SecurityWithPScoresController> logger
        , IRepository<SecurityWithPScore> secScoreRepository)
    {
        this.logger = logger;
        this.secScoreRepository = secScoreRepository;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        IEnumerable<SecurityWithPScore> returnValue = await secScoreRepository.FindAll();
        if (!returnValue.Any())
        {
            return StatusCode(StatusCodes.Status500InternalServerError);
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
            return NotFound();
        }
        return Ok(returnValue);
    }
}