using ApplicationModels.EarningsCal;
using ApplicationModels.Indexes;
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

    public DbSet<IndexComponent> IndexComponents { get; set; }
    public DbSet<EarningsCalendar> EarningsCalendars { get; set; }
    public DbSet<EarningsCalExceptions> EarningsCalExceptions { get; set; }

    #endregion Public Properties

    #region Protected Methods

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseSerialColumns();
    }

    #endregion Protected Methods
}