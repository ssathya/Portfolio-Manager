using ApplicationModels.Indexes;
using Microsoft.EntityFrameworkCore;

namespace PsqlAccess;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseSerialColumns();
    }

    public DbSet<IndexComponent> IndexComponents { get; set; }
}