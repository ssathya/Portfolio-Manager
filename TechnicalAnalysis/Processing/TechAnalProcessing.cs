﻿using ApplicationModels.Compute;
using ApplicationModels.Indexes;
using ApplicationModels.Quotes;
using Microsoft.Extensions.Logging;
using PsqlAccess;

namespace TechnicalAnalysis.Processing;

public class TechAnalProcessing
{
    #region Private Fields

    private const int ApproxWorkingDaysInAYear = 252;
    private const int BatchSize = 20;
    private const int MomentumWindow = 125;
    private readonly IRepository<IndexComponent> idxRepository;
    private readonly ILogger<TechAnalProcessing> logger;
    private readonly IRepository<Momentum> momRepository;
    private readonly List<decimal> xAxis = new();
    private readonly IRepository<YPrice> yRepository;
    private List<YPrice> yPrices = new();

    #endregion Private Fields

    #region Public Constructors

    public TechAnalProcessing(IRepository<YPrice> yRepository
        , IRepository<IndexComponent> idxRepository
        , IRepository<Momentum> momRepository
        , ILogger<TechAnalProcessing> logger)
    {
        this.yRepository = yRepository;
        this.idxRepository = idxRepository;
        this.momRepository = momRepository;
        this.logger = logger;
        for (int i = 1; i <= MomentumWindow; i++)
        {
            xAxis.Add(i);
        }
    }

    #endregion Public Constructors

    #region Public Methods

    public async Task<bool> ExecAsync()
    {
        List<string> tickers = await ObtainTickersToProcessAsync();
        if (!tickers.Any())
        {
            return false;
        }
        await momRepository.Truncate();
        List<Momentum> momentum = new();
        int counter = 0;
        foreach (var ticker in tickers)
        {
            List<CompressedQuote> yQuotes = await ObtainQuotesForTicker(ticker);
            if (yQuotes == null || yQuotes.Count == 0)
            {
                logger.LogInformation($"Could not prices for {ticker}");
                continue;
            }
            Momentum momentumsForTicker = ComputeMomentum(yQuotes, ticker);
            if (momentumsForTicker != null && !string.IsNullOrEmpty(momentumsForTicker.Ticker))
            {
                momentum.Add(momentumsForTicker);
                counter++;
            }
            else
            {
                logger.LogInformation($"Could not compute momentum for {ticker}");
            }
            if (counter % BatchSize == 0)
            {
                await SaveValuesToDatabase(momentum);
                momentum.Clear();
            }
        }
        if (momentum.Any())
        {
            await SaveValuesToDatabase(momentum);
        }
        return true;
    }

    #endregion Public Methods

    #region Private Methods

    private Momentum ComputeMomentum(List<CompressedQuote> yQuotes, string ticker)
    {
        Momentum momentumValues = new();
        if (yQuotes.Count < MomentumWindow)
        {
            return momentumValues;
        }

        momentumValues.Ticker = ticker;
        yQuotes.Sort((a, b) => a.Date.CompareTo(b.Date));

        int iterator = 0;

        while (iterator + MomentumWindow < yQuotes.Count)
        {
            var reportingDt = yQuotes[iterator + MomentumWindow].Date;
            var closingPrices = yQuotes
                .Skip(iterator++)
                .Take(MomentumWindow)
                .Select(x => (Decimal)(Math.Log((double)x.ClosingPrice)))
                .ToArray();
            (decimal rSquared, decimal yIntercept, decimal slope) =
                LinearRegression.LinRegression(xAxis.ToArray(), closingPrices);
            var annualizedSlope = (Math.Pow(Math.Exp((double)slope), ApproxWorkingDaysInAYear) - 1) * 100;
            //Adjust for fitness
            annualizedSlope *= (double)rSquared;
            momentumValues.ComputedValues.Add(new()
            {
                MomentumValue = (decimal)annualizedSlope,
                ReportingDate = reportingDt.ToUniversalTime()
            });
        }
        return momentumValues;
    }

    private async Task<List<CompressedQuote>> ObtainQuotesForTicker(string ticker)
    {
        try
        {
            if (yPrices.Count == 0)
            {
                yPrices = (await yRepository.FindAll())
                    .ToList();
            }
            YPrice? yPrice = yPrices.FindAll(x => x.Ticker == ticker)
                .FirstOrDefault();
            if (yPrice == null)
            {
                return new List<CompressedQuote>();
            }
            return yPrice.CompressedQuotes;
        }
        catch (Exception ex)
        {
            logger.LogError("Exception while retrieving Quotes ObtainQuotesForTicker");
            logger.LogError($"{ex.Message}");
            return new List<CompressedQuote>();
        }
    }

    private async Task<List<string>> ObtainTickersToProcessAsync()
    {
        try
        {
            var tickers = (await idxRepository.FindAll())
                .OrderBy(x => x.Ticker)
                .Select(x => x.Ticker)
                .Distinct()
                .ToList();
            return tickers;
        }
        catch (Exception ex)
        {
            logger.LogError($"Exception while getting all tickers to process ObtainTickersToProcessAsync");
            logger.LogError($"{ex.Message}");
            return new List<string>();
        }
    }

    private async Task SaveValuesToDatabase(List<Momentum> momentum)
    {
        try
        {
            await momRepository.Add(momentum);
        }
        catch (Exception ex)
        {
            logger.LogError("Unable to update database with new values SaveValuesToDatabase");
            logger.LogError($"{ex.Message}");
        }
    }

    #endregion Private Methods
}