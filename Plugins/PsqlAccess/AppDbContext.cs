using ApplicationModels.Compute;
using ApplicationModels.EarningsCal;
using ApplicationModels.FinancialStatement;
using ApplicationModels.FinancialStatement.AlphaVantage;
using ApplicationModels.Indexes;
using ApplicationModels.Quotes;
using ApplicationModels.SimFin;
using ApplicationModels.Views;
using Microsoft.EntityFrameworkCore;

namespace PsqlAccess;

public class AppDbContext : DbContext
{
    #region Public Constructors

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
        Database.EnsureCreated();
    }

    #endregion Public Constructors

    #region Public Properties

    public DbSet<BalanceSheet> BalanceSheets { get; set; }
    public DbSet<CashFlow> CashFlows { get; set; }
    public DbSet<Compute> Computes { get; set; }
    public DbSet<EarningsCalendar> EarningsCalendars { get; set; }
    public DbSet<EarningsCalExceptions> EarningsCalExceptions { get; set; }
    public DbSet<FinStatements> FinStatements { get; set; }
    public DbSet<IncomeStatement> IncomeStatements { get; set; }
    public DbSet<IndexComponent> IndexComponents { get; set; }
    public DbSet<MomMfDolAvg> MomMfDolAvgs { get; set; }
    public DbSet<Overview> Overviews { get; set; }
    public DbSet<ScoreDetail> ScoreDetails { get; set; }
    public DbSet<SecurityWithPScore> SecurityWithPScores { get; set; }
    public DbSet<SimFinRatio> SimFinRatios { get; set; }
    public DbSet<YPrice> YPrices { get; set; }

    #endregion Public Properties

    #region Protected Methods

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseSerialColumns();
        modelBuilder.Entity<BalanceSheet>().HasIndex(p => p.Ticker);
        modelBuilder.Entity<CashFlow>().HasIndex(p => p.Ticker);
        modelBuilder.Entity<Compute>().HasIndex(p => p.Ticker);
        modelBuilder.Entity<EarningsCalendar>().HasIndex(p => p.Ticker);
        modelBuilder.Entity<FinStatements>().HasIndex(p => p.Ticker);
        modelBuilder.Entity<IncomeStatement>().HasIndex(p => p.Ticker);
        modelBuilder.Entity<IndexComponent>().HasIndex(p => p.Ticker);
        modelBuilder.Entity<MomMfDolAvg>().HasIndex(p => p.Ticker);
        modelBuilder.Entity<Overview>().HasIndex(p => p.Ticker);
        modelBuilder.Entity<ScoreDetail>().HasIndex(p => p.Ticker);
        modelBuilder.Entity<SimFinRatio>().HasIndex(p => p.Ticker);
        modelBuilder.Entity<YPrice>().HasIndex(p => p.Ticker);
        modelBuilder.Entity<SecurityWithPScore>(c =>
        {
            c.HasNoKey();
            c.ToView("SecurityWithPScores");
        });
    }

    #endregion Protected Methods
}