using AppCommon.CacheHandler;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

namespace EarningsReport.Processing;

public class GetYahooFinStatements
{
    #region Private Fields

    private const string BalanceSheetUrl = @"https://finance.yahoo.com/quote/symbol/balance-sheet?p=symbol";
    private const string CashFlowUrl = @"https://finance.yahoo.com/quote/symbol/cash-flow?p=symbol";
    private const string DataStoreInnerNode = """//*/div[@data-test="fin-col"] | //*/div[contains(concat(' ', @class, ' '), 'undefined')]/span | //*/div[@class='D(tbhg)']//*/span""";
    private const string IncomeStatementUrl = "https://finance.yahoo.com/quote/symbol/financials?p=symbol";
    private readonly IHandleCache handleCache;
    private readonly ILogger<GetYahooFinStatements> logger;
    private Dictionary<string, decimal> annualReport = new();
    private Dictionary<string, decimal> annualReport1 = new();
    private Dictionary<string, decimal> annualReport2 = new();
    private Dictionary<string, decimal> annualReport3 = new();
    private List<string> headers = new();
    private Dictionary<string, decimal> ttmValues = new();

    #endregion Private Fields

    #region Public Constructors

    //*/div[@class = "D(ib) Va(m) Ell Mt(-3px) W(215px)--mv2 W(200px) undefined"]
    //*/div[@data-test="fin-col"]
    public GetYahooFinStatements(ILogger<GetYahooFinStatements> logger
        , IHandleCache handleCache)
    {
        this.logger = logger;
        this.handleCache = handleCache;
    }

    #endregion Public Constructors

    #region Public Methods

    public async Task<bool> ExecAsync(string ticker)
    {
        if (handleCache == null)
        {
            return false;
        }
        if (!await ProcessIncomeStatement(ticker))
        {
            return false;
        }
        if (!await ProcessCashFlowStatement(ticker))
        {
            return false;
        }
        if (!await ProcessBalanceSheet(ticker))
        {
            return false;
        }
        return true;
    }

    #endregion Public Methods

    #region Private Methods

    private void ClearAccumulators()
    {
        ttmValues.Clear();
        annualReport.Clear();
        annualReport1.Clear();
        annualReport2.Clear();
        annualReport3.Clear();
        headers.Clear();
    }

    private async Task<HtmlDocument> ObtainAndParseExternalData(string ticker, string baseUrl)
    {
        string urlToUse = baseUrl.Replace("symbol", ticker.ToUpper().Trim());
        string extRpt = await handleCache.GetStringAsnyc(urlToUse, CacheDuration.Days, 2, false);
        if (string.IsNullOrEmpty(extRpt))
        {
            logger.LogError("Could not retrieve balance sheet statement");
            return new HtmlDocument();
        }
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(extRpt);
        return htmlDoc;
    }

    private int PopulateHeaders(HtmlNodeCollection tableRows, string breakText)
    {
        int iterator = 0;
        bool dataColumnStarted = false;
        do
        {
            if (iterator == 0)
            {
                if (!tableRows[iterator].InnerText.Equals("Breakdown"))
                {
                    return -1;
                }
            }
            headers.Add(tableRows[++iterator].InnerText);
            if (tableRows[iterator + 1].InnerText.Equals(breakText))
            {
                iterator++;
                dataColumnStarted = true;
            }
        } while (dataColumnStarted == false);
        return iterator;
    }

    private bool PopulateReportValues(HtmlNodeCollection tableRows, int iterator)
    {
        string classification = string.Empty;
        bool processingResult = false;
        int columnCount = headers.Count + 1;
        for (int i = iterator; i < tableRows.Count; i++)
        {
            switch (i % columnCount)
            {
                case 0:
                    classification = tableRows[i].InnerText;
                    processingResult = true;
                    break;

                case 1:
                    processingResult = PopulateValues(tableRows[i], classification, ttmValues);
                    break;

                case 2:
                    processingResult = PopulateValues(tableRows[i], classification, annualReport);
                    break;

                case 3:
                    processingResult = PopulateValues(tableRows[i], classification, annualReport1);
                    break;

                case 4:
                    processingResult = PopulateValues(tableRows[i], classification, annualReport2);
                    break;

                case 5:
                    processingResult = PopulateValues(tableRows[i], classification, annualReport3);
                    break;

                default:
                    processingResult = false;
                    break;
            }
            if (!processingResult)
            {
                return false;
            }
        }
        return processingResult;
    }

    private bool PopulateValues(HtmlNode tableRows, string classification, Dictionary<string, decimal> destinationDictionary)
    {
        var parseResult = Decimal.TryParse(tableRows.InnerText, out decimal value);
        if (!parseResult)
        {
            value = 0;
        }
        try
        {
            destinationDictionary.Add(classification, value);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError($"{ex.Message}");
            return false;
        }
    }

    private async Task<bool> ProcessBalanceSheet(string ticker)
    {
        ClearAccumulators();
        HtmlDocument htmlDoc = await ObtainAndParseExternalData(ticker, BalanceSheetUrl);
        var tableRows = htmlDoc.DocumentNode.SelectNodes(DataStoreInnerNode);

        if (tableRows.Count <= 6)
        {
            logger.LogError("Error reading balance sheet  statement");
            return false;
        }
        int iterator = PopulateHeaders(tableRows, "Total Assets");
        if (iterator == -1)
        {
            logger.LogError("Error in X-Path balance sheet statement");
            return false;
        }
        var processingResult = PopulateReportValues(tableRows, iterator);
        if (!processingResult)
        {
            logger.LogError("Error reading balance sheet statement");
            return false;
        }
        return true;
    }

    private async Task<bool> ProcessCashFlowStatement(string ticker)
    {
        ClearAccumulators();
        var htmlDoc = await ObtainAndParseExternalData(ticker, CashFlowUrl);
        var tableRows = htmlDoc.DocumentNode.SelectNodes(DataStoreInnerNode);

        if (tableRows.Count <= 6)
        {
            logger.LogError("Error reading cash-flow  statement");
            return false;
        }
        int iterator = PopulateHeaders(tableRows, "Operating Cash Flow");
        if (iterator == -1)
        {
            logger.LogError("Error in X-Path cash-flow statement");
            return false;
        }
        var processingResult = PopulateReportValues(tableRows, iterator);
        if (!processingResult)
        {
            logger.LogError("Error reading cash-flow statement");
            return false;
        }

        return true;
    }

    private async Task<bool> ProcessIncomeStatement(string ticker)
    {
        ClearAccumulators();
        var htmlDoc = await ObtainAndParseExternalData(ticker, IncomeStatementUrl);

        HtmlNodeCollection tableRows = htmlDoc.DocumentNode.SelectNodes(DataStoreInnerNode);

        if (tableRows.Count <= 6)
        {
            logger.LogError("Error reading income statement");
            return false;
        }
        int iterator = PopulateHeaders(tableRows, "Total Revenue");
        if (iterator == -1)
        {
            logger.LogError("Error in X-Path for Income statement");
            return false;
        }
        var processingResult = PopulateReportValues(tableRows, iterator);
        if (!processingResult)
        {
            logger.LogError("Error reading income statement");
            return false;
        }

        return true;
    }

    #endregion Private Methods
}