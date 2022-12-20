using ApplicationModels.FinancialStatement;
using ApplicationModels.Indexes;
using Microsoft.Extensions.Logging;
using PsqlAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EarningsReport.Processing;

public class FinStatementsToDb
{
    private readonly IRepository<IndexComponent> idxRepository;
    private readonly IRepository<FinStatements> finStatementRepository;
    private readonly ILogger<FinStatementsToDb> logger;

    public FinStatementsToDb(IRepository<IndexComponent> idxRepository
        , IRepository<FinStatements> finStatementRepository
        , ILogger<FinStatementsToDb> logger)
    {
        this.idxRepository = idxRepository;
        this.finStatementRepository = finStatementRepository;
        this.logger = logger;
    }

    public async Task<bool> ExcecAsync(List<FinStatements> finStatements)
    {
        //1. Delete securities that are not in indexes.
        if (!await RemoveIndexDroppedSecurities())
        {
            return false;
        }
        //2. Delete securities that are to inserted
        if (!await RemoveStatementsToBeRefreshed(finStatements))
        {
            return false;
        }
        //3. Add records to database
        if (!await AddNewRecordsToDb(finStatements))
        {
            return false;
        }
        return true;
    }

    private async Task<bool> AddNewRecordsToDb(List<FinStatements> finStatements)
    {
        try
        {
            await finStatementRepository.Add(finStatements);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError("Unable to add records FinStatementsToDb:AddNewRecordsToDb");
            logger.LogError($"{ex.Message}");
            return false;
        }
        throw new NotImplementedException();
    }

    private async Task<bool> RemoveStatementsToBeRefreshed(List<FinStatements> finStatements)
    {
        List<string> tickersToRemove = finStatements.Select(x => x.Ticker)
            .Distinct()
            .ToList();
        try
        {
            foreach (var droppedTicker in tickersToRemove)
            {
                var dropRecords = await finStatementRepository.FindAll(x => x.Ticker == droppedTicker);
                await finStatementRepository.Remove(dropRecords);
            }
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError("Unable to drop securities FinStatementsToDb:RemoveStatementsToBeRefreshed");
            logger.LogError($"{ex.Message}");
            return false;
        }
    }

    private async Task<bool> RemoveIndexDroppedSecurities()
    {
        try
        {
            List<string> tickers = (await idxRepository.FindAll())
                .Select(x => x.Ticker)
                .ToList();
            List<string> tickersInReport = (await finStatementRepository.FindAll())
                .Select(x => x.Ticker)
                .Distinct()
                .ToList();
            IEnumerable<string> droppedTickers = tickersInReport.Except(tickers);
            foreach (var droppedTicker in droppedTickers)
            {
                var dropRecords = await finStatementRepository.FindAll(x => x.Ticker == droppedTicker);
                await finStatementRepository.Remove(dropRecords);
            }
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError("Unable to drop securities that are not in index FinStatementsToDb:RemoveIndexDroppedSecurities");
            logger.LogError($"{ex.Message}");
            return false;
        }
    }
}