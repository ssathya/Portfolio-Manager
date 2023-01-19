using ApplicationModels.Compute;
using Microsoft.AspNetCore.Mvc;
using PsqlAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PresentationHelper.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ComputesController : ControllerBase
{
    private readonly ILogger<ComputesController> logger;
    private readonly IRepository<Compute> repository;

    public ComputesController(ILogger<ComputesController> logger
        , IRepository<Compute> repository)
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
        Compute? compute = (await repository.FindAll(r => r.Ticker == ticker.ToUpper().Trim())).FirstOrDefault();
        if (compute == null)
        {
            return StatusCode(StatusCodes.Status404NotFound);
        }
        return Ok(compute);
    }
}