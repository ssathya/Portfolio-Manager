using ApplicationModels.FinancialStatement.AlphaVantage;
using PsqlAccess;

namespace Presentation.Data;

public class IncomeStatementService
{
    private readonly ILogger<IncomeStatementService> logger;
    private readonly IRepository<IncomeStatement> repository;

    public IncomeStatementService(ILogger<IncomeStatementService> logger
        , IRepository<IncomeStatement> repository)
    {
        this.logger = logger;
        this.repository = repository;
    }

    public async Task<IncomeStatement?> ExecAsync(string ticker)
    {
        if (string.IsNullOrEmpty(ticker)) return null;

        try
        {
            IncomeStatement? incomeStatement = (await repository.FindAll(r => r.Ticker == ticker.ToUpper().Trim())).FirstOrDefault();
            if (incomeStatement == null)
            {
                logger.LogInformation($"Income statement for security {ticker} not in database");
                return null;
            }
            return incomeStatement;
        }
        catch (Exception ex)
        {
            logger.LogError($"Error reading income statement from database for ticker {ticker}");
            logger.LogError(message: ex.ToString());
            return null;
        }
    }
}