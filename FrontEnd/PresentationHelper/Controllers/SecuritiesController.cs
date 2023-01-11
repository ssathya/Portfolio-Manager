using ApplicationModels.Indexes;
using Microsoft.AspNetCore.Mvc;
using PsqlAccess;

namespace PresentationHelper.Controllers;

[ApiController]
[Route("[Controller]")]
public class SecuritiesController : ControllerBase
{
    private readonly ILogger<SecuritiesController> logger;
    private readonly IRepository<IndexComponent> icRepository;

    public SecuritiesController(ILogger<SecuritiesController> logger
        , IRepository<IndexComponent> icRepository)
    {
        this.logger = logger;
        this.icRepository = icRepository;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        IEnumerable<IndexComponent> returnValue = await icRepository.FindAll();
        return Ok(returnValue);
    }
}