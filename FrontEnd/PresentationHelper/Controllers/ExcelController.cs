using ApplicationModels.Compute;
using ApplicationModels.ViewModel;
using ApplicationModels.Views;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using OfficeOpenXml.Table;
using PsqlAccess;

namespace PresentationHelper.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ExcelController : ControllerBase
{
    private readonly ILogger<ExcelController> logger;
    private readonly IRepository<SecurityWithPScore> secScoreRepository;
    private readonly IRepository<MomMfDolAvg> momentumRepository;

    public ExcelController(ILogger<ExcelController> logger
        , IRepository<SecurityWithPScore> secScoreRepository
        , IRepository<MomMfDolAvg> momentumRepository)
    {
        this.logger = logger;
        this.secScoreRepository = secScoreRepository;
        this.momentumRepository = momentumRepository;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        IEnumerable<SecurityWithPScore>? dbSecValues = null;
        IEnumerable<MomMfDolAvg>? momMfDolAvgs = null;
        try
        {
            dbSecValues = await secScoreRepository.FindAll();
            momMfDolAvgs = await momentumRepository.FindAll();
        }
        catch (Exception ex)
        {
            logger.LogError($"Error extracting values from Db. Excel generation failed");
            logger.LogError(ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
        List<SecurityDetails> securityDetails = new();
        PopulateSecurityDetails(securityDetails, dbSecValues, momMfDolAvgs);
        ExcelPackage p;
        MemoryStream exportStream;
        byte[] strOut;
        GenerateExcel(securityDetails, out p, out exportStream, out strOut);
        return new FileContentResult(strOut, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
    }

    private static void GenerateExcel(List<SecurityDetails> securityDetails, out ExcelPackage p, out MemoryStream exportStream, out byte[] strOut)
    {
        p = new ExcelPackage();
        p.Workbook.Properties.Title = "Selected Firms";
        p.Workbook.Properties.Author = "Linux system";
        ExcelWorksheet ws = p.Workbook.Worksheets.Add("Extract");
        ws.Cells[1, 1].LoadFromCollection(securityDetails, true, TableStyles.Medium12);
        exportStream = new MemoryStream();
        p.SaveAs(exportStream);
        exportStream.Position = 0;
        strOut = exportStream.ToArray();
    }

    private static void PopulateSecurityDetails(List<SecurityDetails> securityDetails, IEnumerable<SecurityWithPScore>? dbSecValues, IEnumerable<MomMfDolAvg>? momMfDolAvgs)
    {
        foreach (var dbSecValue in dbSecValues)
        {
            securityDetails.Add(dbSecValue);
            var secDetail = securityDetails.Last();
            MomMfDolAvg? mfDolAvgs = momMfDolAvgs.FirstOrDefault(x => x.Ticker == dbSecValue.Ticker);
            if (mfDolAvgs != null)
            {
                secDetail.DollarVolume = mfDolAvgs.DollarVolume;
                secDetail.Momentum = mfDolAvgs.Momentum;
                secDetail.MoneyFlow = mfDolAvgs.MoneyFlow;
            }
        }
    }
}