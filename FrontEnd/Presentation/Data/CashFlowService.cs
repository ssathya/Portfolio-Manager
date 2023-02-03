using ApplicationModels.FinancialStatement.AlphaVantage;
using PsqlAccess;

namespace Presentation.Data;

public class CashFlowService
{
    private readonly ILogger<CashFlowService> logger;
    private readonly IRepository<CashFlow> repository;

    public CashFlowService(ILogger<CashFlowService> logger
        , IRepository<CashFlow> repository)
    {
        this.logger = logger;
        this.repository = repository;
    }

    public async Task<CashFlow?> ExecAsync(string ticker)
    {
        if (string.IsNullOrEmpty(ticker)) return null;

        try
        {
            CashFlow? cashFlow = (await repository.FindAll(r => r.Ticker == ticker.ToUpper().Trim())).FirstOrDefault();
            if (cashFlow == null)
            {
                logger.LogInformation($"Cash flow for security {ticker} not in database");
                return null;
            }
            return cashFlow;
        }
        catch (Exception ex)
        {
            logger.LogError($"Error reading cash flow from database for ticker {ticker}");
            logger.LogError(message: ex.ToString());
            return null;
        }
    }
}