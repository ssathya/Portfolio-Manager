using ApplicationModels.Compute;
using ApplicationModels.EarningsCal;
using ApplicationModels.FinancialStatement;
using ApplicationModels.Indexes;
using ApplicationModels.Quotes;
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

    public DbSet<Compute> Computes { get; set; }
    public DbSet<EarningsCalendar> EarningsCalendars { get; set; }
    public DbSet<EarningsCalExceptions> EarningsCalExceptions { get; set; }
    public DbSet<FinStatements> FinStatements { get; set; }
    public DbSet<IndexComponent> IndexComponents { get; set; }
    public DbSet<ScoreDetail> ScoreDetails { get; set; }
    public DbSet<YPrice> YPrices { get; set; }

    #endregion Public Properties

    #region Protected Methods

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseSerialColumns();
        modelBuilder.Entity<Compute>().HasIndex(p => p.Ticker);
        modelBuilder.Entity<EarningsCalendar>().HasIndex(p => p.Ticker);
        modelBuilder.Entity<FinStatements>().HasIndex(p => p.Ticker);
        modelBuilder.Entity<IndexComponent>().HasIndex(p => p.Ticker);
        modelBuilder.Entity<ScoreDetail>().HasIndex(p => p.Ticker);
        modelBuilder.Entity<YPrice>().HasIndex(p => p.Ticker);
    }

    #endregion Protected Methods
}