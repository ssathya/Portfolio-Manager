using ApplicationModels.FinancialStatement.AlphaVantage;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using OfficeOpenXml.Table;
using PsqlAccess;

namespace Presentation.Data;

public class ExcelServiceFinancial
{
    private readonly ILogger<ExcelServiceFinancial> logger;
    private readonly IRepository<BalanceSheet> balanceSheetRepository;
    private readonly IRepository<CashFlow> cashFlowRepository;
    private readonly IRepository<IncomeStatement> incomeStatementRepository;

    public ExcelServiceFinancial(ILogger<ExcelServiceFinancial> logger
        , IRepository<BalanceSheet> balanceSheetRepository
        , IRepository<CashFlow> cashFlowRepository
        , IRepository<IncomeStatement> incomeStatementRepository)
    {
        this.logger = logger;
        this.balanceSheetRepository = balanceSheetRepository;
        this.cashFlowRepository = cashFlowRepository;
        this.incomeStatementRepository = incomeStatementRepository;
    }

    public async Task<FileContentResult>? ExecAsync(string ticker)
    {
        BalanceSheet? balanceSheets = new();
        CashFlow? cashFlows = new();
        IncomeStatement? incomeStatements = new();

        try
        {
            balanceSheets = (await balanceSheetRepository.FindAll(r => r.Ticker == ticker))
               .FirstOrDefault();
            cashFlows = (await cashFlowRepository.FindAll(r => r.Ticker == ticker))
                .FirstOrDefault();
            incomeStatements = (await incomeStatementRepository.FindAll(r => r.Ticker == ticker))
                .FirstOrDefault();
        }
        catch (Exception ex)
        {
            logger.LogError($"Error reading financial statements from database for ticker {ticker}");
            logger.LogError(ex.Message);
#pragma warning disable CS8603 // Possible null reference return.
            return null;
#pragma warning restore CS8603 // Possible null reference return.
        }
        if (balanceSheets != null && cashFlows != null && incomeStatements != null)
        {
            GenerateExcel(balanceSheets, incomeStatements, cashFlows
                , out ExcelPackage p, out MemoryStream exportStream, out byte[] strOut);
            return new FileContentResult(strOut, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        }
#pragma warning disable CS8603 // Possible null reference return.
        return null;
#pragma warning restore CS8603 // Possible null reference return.
    }

    private static void GenerateExcel(BalanceSheet balanceSheets, IncomeStatement incomeStatements
        , CashFlow cashFlows, out ExcelPackage p
        , out MemoryStream exportStream, out byte[] strOut)
    {
        p = new ExcelPackage();
        p.Workbook.Properties.Title = "Balance Sheets";
        p.Workbook.Properties.Author = "Linux System";

        ExcelWorksheet wsBS = p.Workbook.Worksheets.Add("Balance Sheets");
        wsBS.Cells[1, 1].LoadFromCollection(balanceSheets.AnnualReports, true, TableStyles.Medium12);

        ExcelWorksheet wsIS = p.Workbook.Worksheets.Add("Income Statements");
        wsIS.Cells[1, 1].LoadFromCollection(incomeStatements.AnnualReports, true, TableStyles.Medium12);

        ExcelWorksheet wsCF = p.Workbook.Worksheets.Add("Cash Flows");
        wsCF.Cells[1, 1].LoadFromCollection(cashFlows.AnnualReports, true, TableStyles.Medium12);

        exportStream = new MemoryStream();
        p.SaveAs(exportStream);
        exportStream.Position = 0;
        strOut = exportStream.ToArray();
    }
}