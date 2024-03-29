﻿using ApplicationModels.EarningsCal;
using ApplicationModels.FinancialStatement;
using AutoMapper;
using Microsoft.Extensions.Logging;
using PsqlAccess;
using SecuritiesExchangeCommission.Edgar;
using Xbrl;
using Xbrl.FinancialStatement;
using Xbrl.Helpers;

namespace EarningsReport.Processing;

public class GetFinancialStatements
{
    #region Private Fields

    private const int MaxNumberOfCalls = 20;
    private const int QuarterCount = 4;
    private const int YearCount = 3;
    private readonly ILogger<GetFinancialStatements> logger;
    private readonly IMapper mapper;
    private readonly IRepository<EarningsCalendar> ecRepository;

    #endregion Private Fields

    #region Public Constructors

    public GetFinancialStatements(IRepository<EarningsCalendar> ecRepository
        , ILogger<GetFinancialStatements> logger
        , IMapper mapper)
    {
        this.ecRepository = ecRepository;
        this.logger = logger;
        this.mapper = mapper;
        SecRequestManager.Instance.UserAgent = "Adept/4.1.0";
    }

    #endregion Public Constructors

    #region Public Methods

    public async Task<List<FinStatements>> ExcecAsync()
    {
        List<EarningsCalendar> earningsCalendars = await GetFirmsToObtainEarningsReportAsync();
        List<FinStatements> finStatements = new();
        if (earningsCalendars.Count == 0)
        {
            return finStatements;
        }

        foreach (var earningsCalendar in earningsCalendars)
        {
            List<FinStatements> statements = await GetFinancialAsync(earningsCalendar.Ticker);
            if (statements.Count >= 0)
            {
                finStatements.AddRange(statements);
            }
        }
        var processedTickers = finStatements.Select(x => x.Ticker).Distinct().ToList();
        if (processedTickers.Count == 0)
        {
            return finStatements;
        }
        return finStatements;
    }

    #endregion Public Methods

    #region Private Methods

    private async Task<List<FinStatements>> ExtractValuesAsync(EdgarSearch edgarSearch, int extractCount, string ticker, string filingType)
    {
        var univeralDefaultTime = new DateTime(1900, 01, 01).ToUniversalTime();
        List<FinStatements> result = new();
        for (int i = 0; i < extractCount; i++)
        {
            XbrlInstanceDocument? doc;
            Stream s;
            try
            {
                s = await edgarSearch.Results[i].DownloadXbrlDocumentAsync();
                StreamReader srCheck = new(s);
                string xml = srCheck.ReadToEnd();
                if (!string.IsNullOrEmpty(xml))
                {
                    await File.WriteAllTextAsync($"{ticker}.xml", xml);
                    doc = XbrlInstanceDocument.Create(s);
                }
                else
                {
                    doc = null;
                }
                if (doc != null)
                {
                    FinancialStatement financialStatement = doc.CreateFinancialStatement();
                    var focus_context = doc.FindNormalPeriodPrimaryContext();

                    FinStatements statements = mapper.Map<FinStatements>(financialStatement);
                    float longTermDebt = doc.GetValueFromPriorities(focus_context.Id,
                        "LongTermDebtAndCapitalLeaseObligationsCurrent",
                        "LongTermDebtAndCapitalLeaseObligationsIncludingCurrentMaturities",
                        "LongTermDebtCurrent",
                        "LongTermDebtMaturityDate",
                        "LongTermDebtFairValue").ValueAsFloat();
                    if (longTermDebt != 0)
                    {
                        statements.LongTermDebt = longTermDebt;
                    }
                    statements.Ticker = ticker;
                    statements.FilingType = filingType;
                    if (statements.PeriodStart != null)
                    {
                        statements.PeriodStart = statements.PeriodStart.Value.ToUniversalTime();
                    }
                    else
                    {
                        statements.PeriodStart = univeralDefaultTime;
                    }
                    if (statements.PeriodEnd != null)
                    {
                        statements.PeriodEnd = statements.PeriodEnd.Value.ToUniversalTime();
                    }
                    else
                    {
                        statements.PeriodEnd = univeralDefaultTime;
                    }
                    result.Add(statements);

                    logger.LogInformation($"Processed {ticker} for period {statements.PeriodStart:MM-dd-yyyy} to {statements.PeriodEnd:MM-dd-yyyy}");
                }
            }
            catch (Exception ex)
            {
                logger.LogError("Error while reading data from EDGAR");
                logger.LogError(ex.ToString());
                result.Clear();
                return result;
            }
        }
        return result;
    }

    private async Task<List<FinStatements>> GetFinancialAsync(string ticker)
    {
        List<FinStatements> finStatements = new();

        EdgarSearch edgarSearch;
        try
        {
            edgarSearch = await EdgarSearch.CreateAsync(ticker, "10-K");
        }
        catch (Exception ex)
        {
            logger.LogError($"Error occurred while trying to retrieve list of 10-K filings for {ticker}");
            logger.LogError(ex.Message);
            return finStatements;
        }
        int availableYears = Math.Min(YearCount, edgarSearch.Results.Length);
        logger.LogInformation($"Going to obtain {availableYears} 10-K records for {ticker}");
        finStatements.AddRange(await ExtractValuesAsync(edgarSearch, availableYears, ticker, "10-K"));
        if (finStatements.Count == 0)
        {
            return finStatements;
        }
        try
        {
            edgarSearch = await EdgarSearch.CreateAsync(ticker, "10-Q");
        }
        catch (Exception ex)
        {
            logger.LogError($"Error occurred while trying to retrieve list of 10-Q filings for {ticker}");
            logger.LogError(ex.Message);
            return finStatements;
        }
        int requiredQuarters = Math.Min(QuarterCount, edgarSearch.Results.Length);
        logger.LogInformation($"Going to obtain {requiredQuarters} 10-Q records for {ticker}");
        var extractResult = await ExtractValuesAsync(edgarSearch, requiredQuarters, ticker, "10-Q");
        if (extractResult.Count == 0)
        {
            return extractResult;
        }
        finStatements.AddRange(extractResult);
        return finStatements;
    }

    private async Task<List<EarningsCalendar>> GetFirmsToObtainEarningsReportAsync()
    {
        try
        {
            var ecRecords = (await ecRepository.FindAll(x => x.DataObtained == false))
                .OrderBy(x => x.VendorEarningsDate)
                .ThenBy(x => x.EarningsDateYahoo)
                .Take(MaxNumberOfCalls)
                .ToList();
            return ecRecords;
        }
        catch (Exception ex)
        {
            logger.LogError($"Error while reading database GetFinancialStatements:GetFirmsToObtainEarningsReport");
            logger.LogError($"{ex.Message}", ex);
            return new List<EarningsCalendar>();
        }
    }

    #endregion Private Methods
}