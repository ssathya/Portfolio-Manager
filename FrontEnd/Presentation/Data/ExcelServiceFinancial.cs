using ApplicationModels.FinancialStatement.AlphaVantage;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using OfficeOpenXml.Table;
using PsqlAccess;
using System.Data;
using System.Reflection;

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
            GenerateExcelNew(balanceSheets, incomeStatements, cashFlows
                , out ExcelPackage p1, out MemoryStream exportStream1, out byte[] strOut1);
            //GenerateExcel(balanceSheets, incomeStatements, cashFlows
            //    , out ExcelPackage p, out MemoryStream exportStream, out byte[] strOut);
            return new FileContentResult(strOut1, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        }
#pragma warning disable CS8603 // Possible null reference return.
        return null;
#pragma warning restore CS8603 // Possible null reference return.
    }

    private void GenerateExcelNew(BalanceSheet balanceSheets, IncomeStatement incomeStatements, CashFlow cashFlows,
        out ExcelPackage p, out MemoryStream exportStream, out byte[] strOut)
    {
        DataTable bsTable = new();
        PopulateBsTable(balanceSheets, bsTable);

        DataTable isTable = new();
        PopulateIncomeStatementTable(incomeStatements, isTable);

        DataTable cfTable = new();
        PopulateCashFlowTable(cashFlows, cfTable);
        p = new ExcelPackage();

        try
        {
            p.Workbook.Properties.Title = "Balance Sheets";
            p.Workbook.Properties.Author = "Linux System";
            ExcelWorksheet wsBS = p.Workbook.Worksheets.Add("Balance Sheets");
            wsBS.Cells[1, 1].LoadFromDataTable(bsTable, true, TableStyles.Medium12)
                .AutoFitColumns();
            ExcelWorksheet wsCF = p.Workbook.Worksheets.Add("Cash Flows");
            wsCF.Cells[1, 1].LoadFromDataTable(cfTable, true, TableStyles.Medium12)
                .AutoFitColumns();
            ExcelWorksheet wsIS = p.Workbook.Worksheets.Add("Income Statements");
            wsIS.Cells[1, 1].LoadFromDataTable(isTable, true, TableStyles.Medium12)
                .AutoFitColumns();
        }
        catch (Exception ex)
        {
            logger.LogError("Error generating excel s/s");
            logger.LogError($"{ex.Message}");
        }

        exportStream = new MemoryStream();
        p.SaveAs(exportStream);
        exportStream.Position = 0;
        strOut = exportStream.ToArray();
    }

    private static void PopulateCashFlowTable(CashFlow cashFlows, DataTable cfTable)
    {
        IEnumerable<PropertyInfo> properties = from p in typeof(CashFlowReport).GetProperties()
                                               where p.PropertyType == typeof(decimal) || p.PropertyType == typeof(decimal?)
                                               select p;
        if (properties == null || !properties.Any())
        {
            return;
        }
        DataColumn col = new()
        {
            DataType = typeof(string),
            ColumnName = "Values",
            Caption = "Attributes"
        };
        cfTable.Columns.Add(col);

        foreach (var cashFlowReport in cashFlows.AnnualReports.Take(4))
        {
            col = new()
            {
                DataType = typeof(decimal),
                ColumnName = cashFlowReport.FiscalDateEnding.ToString("yyyy-MM-dd"),
                Caption = cashFlowReport.FiscalDateEnding.ToString("yyyy-MM-dd")
            };
            cfTable.Columns.Add(col);
        }
        foreach (var cashFlowReport in cashFlows.QuarterlyReports.Take(4))
        {
            col = new()
            {
                DataType = typeof(decimal),
                ColumnName = cashFlowReport.FiscalDateEnding.ToString("yyyy-MM-dd(Q)"),
                Caption = cashFlowReport.FiscalDateEnding.ToString("yyyy-MM-dd(Q)")
            };
            cfTable.Columns.Add(col);
        }
        foreach (var property in properties)
        {
            DataRow row = cfTable.Rows.Add();
            row[0] = property.Name.Humanize().Transform(To.TitleCase);
            int colIdx = 1;
            foreach (var bsReport in cashFlows.AnnualReports.Take(4))
            {
                row[colIdx++] = (decimal?)property.GetValue(bsReport) ?? 0.0M;
            }
            foreach (var bsReport in cashFlows.QuarterlyReports.Take(4))
            {
                row[colIdx++] = (decimal?)property.GetValue(bsReport) ?? 0.0M;
            }
        }
    }

    private static void PopulateBsTable(BalanceSheet balanceSheets, DataTable bsTable)
    {
        IEnumerable<PropertyInfo> properties = from p in typeof(BSReport).GetProperties()
                                               where p.PropertyType == typeof(decimal) || p.PropertyType == typeof(decimal?)
                                               select p;
        if (properties == null || !properties.Any())
        {
            return;
        }
        DataColumn col = new()
        {
            DataType = typeof(string),
            ColumnName = "Values",
            Caption = "Attributes"
        };
        bsTable.Columns.Add(col);

        foreach (var bsReport in balanceSheets.AnnualReports.Take(4))
        {
            col = new()
            {
                DataType = typeof(decimal),
                ColumnName = bsReport.FiscalDateEnding.ToString("yyyy-MM-dd"),
                Caption = bsReport.FiscalDateEnding.ToString("yyyy-MM-dd")
            };
            bsTable.Columns.Add(col);
        }
        foreach (var bsReport in balanceSheets.QuarterlyReports.Take(4))
        {
            col = new()
            {
                DataType = typeof(decimal),
                ColumnName = bsReport.FiscalDateEnding.ToString("yyyy-MM-dd(Q)"),
                Caption = bsReport.FiscalDateEnding.ToString("yyyy-MM-dd(Q)")
            };
            bsTable.Columns.Add(col);
        }

        foreach (var property in properties)
        {
            DataRow row = bsTable.Rows.Add();
            row[0] = property.Name.Humanize().Transform(To.TitleCase);
            int colIdx = 1;
            foreach (var bsReport in balanceSheets.AnnualReports.Take(4))
            {
                row[colIdx++] = (decimal?)property.GetValue(bsReport) ?? 0.0M;
            }
            foreach (var bsReport in balanceSheets.QuarterlyReports.Take(4))
            {
                row[colIdx++] = (decimal?)property.GetValue(bsReport) ?? 0.0M;
            }
        }
    }

    private static void PopulateIncomeStatementTable(IncomeStatement incomeStatements, DataTable isTable)
    {
        IEnumerable<PropertyInfo> properties = from p in typeof(IncomeReport).GetProperties()
                                               where p.PropertyType == typeof(decimal) || p.PropertyType == typeof(decimal?)
                                               select p;
        if (properties == null || !properties.Any())
        {
            return;
        }
        DataColumn col = new()
        {
            DataType = typeof(string),
            ColumnName = "Values",
            Caption = "Attributes"
        };
        isTable.Columns.Add(col);

        foreach (var incomeReport in incomeStatements.AnnualReports.Take(4))
        {
            col = new()
            {
                DataType = typeof(decimal),
                ColumnName = incomeReport.FiscalDateEnding.ToString("yyyy-MM-dd"),
                Caption = incomeReport.FiscalDateEnding.ToString("yyyy-MM-dd")
            };
            isTable.Columns.Add(col);
        }
        foreach (var incomeReport in incomeStatements.QuarterlyReports.Take(4))
        {
            col = new()
            {
                DataType = typeof(decimal),
                ColumnName = incomeReport.FiscalDateEnding.ToString("yyyy-MM-dd(Q)"),
                Caption = incomeReport.FiscalDateEnding.ToString("yyyy-MM-dd(Q)")
            };
            isTable.Columns.Add(col);
        }
        foreach (var property in properties)
        {
            DataRow row = isTable.Rows.Add();
            row[0] = property.Name.Humanize().Transform(To.TitleCase);
            int colIdx = 1;
            foreach (var incomeReport in incomeStatements.AnnualReports.Take(4))
            {
                row[colIdx++] = (decimal?)property.GetValue(incomeReport) ?? 0.0M;
            }
            foreach (var incomeReport in incomeStatements.QuarterlyReports.Take(4))
            {
                row[colIdx++] = (decimal?)property.GetValue(incomeReport) ?? 0.0M;
            }
        }
    }

    private static void GenerateExcel(BalanceSheet balanceSheets, IncomeStatement incomeStatements
        , CashFlow cashFlows, out ExcelPackage p
        , out MemoryStream exportStream, out byte[] strOut)
    {
        p = new ExcelPackage();
        p.Workbook.Properties.Title = "Balance Sheets";
        p.Workbook.Properties.Author = "Linux System";

        List<BSReport> bSReports = new();
        bSReports.AddRange(balanceSheets.AnnualReports);
        bSReports.AddRange(balanceSheets.QuarterlyReports);
        ExcelWorksheet wsBS = p.Workbook.Worksheets.Add("Balance Sheets");
        wsBS.Cells[100, 1].LoadFromCollection(bSReports, true, TableStyles.None)
            .AutoFitColumns();
        wsBS.ProtectedRanges.Clear();
        bSReports.Clear();

        List<CashFlowReport> cashFlowReports = new();
        cashFlowReports.AddRange(cashFlows.AnnualReports);
        cashFlowReports.AddRange(cashFlows.QuarterlyReports);
        ExcelWorksheet wsIS = p.Workbook.Worksheets.Add("Income Statements");
        wsIS.Cells[1, 1].LoadFromCollection(cashFlowReports, true, TableStyles.None)
            .AutoFitColumns();
        cashFlowReports.Clear();

        List<IncomeReport> incomeReports = new();
        incomeReports.AddRange(incomeStatements.AnnualReports);
        incomeReports.AddRange(incomeStatements.QuarterlyReports);
        ExcelWorksheet wsCF = p.Workbook.Worksheets.Add("Cash Flows");
        wsCF.Cells[1, 1].LoadFromCollection(incomeReports, true, TableStyles.None)
            .AutoFitColumns();
        incomeReports.Clear();

        exportStream = new MemoryStream();
        p.SaveAs(exportStream);
        exportStream.Position = 0;
        strOut = exportStream.ToArray();
    }
}