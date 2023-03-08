using ApplicationModels.Compute;
using ApplicationModels.ViewModel;
using ApplicationModels.Views;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using OfficeOpenXml.Table;
using PsqlAccess;

namespace Presentation.Data;

public class ExcelServiceAllSecurities
{
    private readonly ILogger<ExcelServiceAllSecurities> logger;
    private readonly IRepository<MomMfDolAvg> momentumRepository;
    private readonly IRepository<SecurityWithPScore> secScoreRepository;

    public ExcelServiceAllSecurities(ILogger<ExcelServiceAllSecurities> logger
        , IRepository<SecurityWithPScore> secScoreRepository
        , IRepository<MomMfDolAvg> momentumRepository)
    {
        this.logger = logger;
        this.secScoreRepository = secScoreRepository;
        this.momentumRepository = momentumRepository;
    }

    public async Task<FileContentResult>? ExecAsync()
    {
        IEnumerable<SecurityWithPScore>? dbSecValues;
        IEnumerable<MomMfDolAvg>? momMfDolAvgs;
        try
        {
            dbSecValues = await secScoreRepository.FindAll();
            momMfDolAvgs = await momentumRepository.FindAll();
        }
        catch (Exception ex)
        {
            logger.LogError($"Error extracting values from Db. Excel generation failed");
            logger.LogError(ex.Message);
#pragma warning disable CS8603 // Possible null reference return.
            return null;
#pragma warning restore CS8603 // Possible null reference return.
        }
        List<SecurityDetails> securityDetails = new();
        PopulateSecurityDetails(securityDetails, dbSecValues, momMfDolAvgs);
        GenerateExcel(securityDetails, out ExcelPackage p, out MemoryStream exportStream, out byte[] strOut);
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
        dbSecValues ??= new List<SecurityWithPScore>();
        momMfDolAvgs ??= new List<MomMfDolAvg>();
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