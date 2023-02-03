using ApplicationModels.FinancialStatement.AlphaVantage;
using PsqlAccess;

namespace Presentation.Data;

public class BalanceSheetService
{
    private readonly ILogger<BalanceSheetService> logger;
    private readonly IRepository<BalanceSheet> repository;

    public BalanceSheetService(ILogger<BalanceSheetService> logger
        , IRepository<BalanceSheet> repository)
    {
        this.logger = logger;
        this.repository = repository;
    }

    public async Task<BalanceSheet?> ExecAsync(string ticker)
    {
        if (string.IsNullOrEmpty(ticker)) return null;

        try
        {
            BalanceSheet? balanceSheet = (await repository.FindAll(r => r.Ticker == ticker.ToUpper().Trim())).FirstOrDefault();
            if (balanceSheet == null)
            {
                logger.LogInformation($"Balance sheet for security {ticker} not in database");
                return null;
            }
            return balanceSheet;
        }
        catch (Exception ex)
        {
            logger.LogError($"Error reading balance sheet from database for ticker {ticker}");
            logger.LogError(ex.ToString());
            return null;
        }
    }
}