using ApplicationModels;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace PsqlAccess.SecListMaintain;

public class GenericRepository<T> : IRepository<T> where T : Entity
{
    private readonly IDbContextFactory<AppDbContext> contextFactory;
    private readonly ILogger<GenericRepository<T>> logger;

    public GenericRepository(IDbContextFactory<AppDbContext> contextFactory, ILogger<GenericRepository<T>> logger)
    {
        this.contextFactory = contextFactory;
        this.logger = logger;
    }

    public async Task Add(T entity)
    {
        using AppDbContext dbCondext = await contextFactory.CreateDbContextAsync();
        try
        {
            dbCondext.Add(entity);
            await dbCondext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError("Error in Adding a value to database ");
            logger.LogError(ex.Message);
        }
    }

    public async Task Add(IEnumerable<T> entities)
    {
        using AppDbContext dbCondext = await contextFactory.CreateDbContextAsync();
        try
        {
            using var transaction = dbCondext.Database.BeginTransaction();
            var entities1 = entities.ToList();
            var bulkConfig = new BulkConfig { SetOutputIdentity = true, BatchSize = 4000 };
            await dbCondext.BulkInsertAsync<T>(entities1, bulkConfig);
            transaction.Commit();
            //await dbCondext.AddRangeAsync(entities);
            //await dbCondext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError("Error in Adding values to database ");
            logger.LogError(ex.Message);
        }
    }

    public async Task<IEnumerable<T>> FindAll(params Expression<Func<T, object>>[] includeProperties)
    {
        using AppDbContext dbCondext = await contextFactory.CreateDbContextAsync();
        IQueryable<T> items = dbCondext.Set<T>();
        if (includeProperties == null || includeProperties.Length == 0)
        {
            return await items.ToListAsync();
        }
        foreach (var includeProperty in includeProperties)
        {
            items = items.Include(includeProperty);
        }
        return await items.ToListAsync();
    }

    public async Task<IEnumerable<T>> FindAll(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includeProperties)
    {
        //var items = await FindAll(includeProperties);
        using AppDbContext dbCondext = await contextFactory.CreateDbContextAsync();
        IQueryable<T> items = dbCondext.Set<T>();
        if (includeProperties != null && includeProperties.Length != 0)
        {
            foreach (var includeProperty in includeProperties)
            {
                items = items.Include(includeProperty);
            }
        }
        return await items.Where(predicate).ToListAsync();
    }

    public async Task<T?> FindById(int id, params Expression<Func<T, object>>[] includeProperties)
    {
        using AppDbContext dbCondext = await contextFactory.CreateDbContextAsync();
        var items = dbCondext.Set<T>();
        if (includeProperties != null)
        {
            foreach (var includeProperty in includeProperties)
            {
                items.Include(includeProperty);
            }
        }
        return await items.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task Remove(T entity)
    {
        using AppDbContext dbCondext = await contextFactory.CreateDbContextAsync();
        try
        {
            dbCondext.Remove(entity);
            await dbCondext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError("Error in deleting value from database ");
            logger.LogError(ex.Message);
        }
    }

    public async Task Remove(IEnumerable<T> entities)
    {
        using AppDbContext appDbConext = await contextFactory.CreateDbContextAsync();
        var ids = entities.Select(x => x.Id).ToList();
        try
        {
            using var transaction = appDbConext.Database.BeginTransaction();
            var entities1 = entities.ToList();
            var bulkConfig = new BulkConfig { SetOutputIdentity = true, BatchSize = 4000 };
            await appDbConext.BulkDeleteAsync<T>(entities1, bulkConfig);
            transaction.Commit();
            //await appDbConext.Set<T>()
            //    .Where(x => ids.Contains(x.Id))
            //    .ExecuteDeleteAsync();
            //await appDbConext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError("Error in deleting values from database ");
            logger.LogError(ex.Message);
        }
    }

    public async Task Remove(int id)
    {
        using AppDbContext appDbConext = await contextFactory.CreateDbContextAsync();
        try
        {
            appDbConext.Remove(id);
            await appDbConext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError("Error in deleting values from database ");
            logger.LogError(ex.Message);
        }
    }

    public async Task Truncate()
    {
        using AppDbContext appDbConext = await contextFactory.CreateDbContextAsync();
        try
        {
            using var transaction = appDbConext.Database.BeginTransaction();
            await appDbConext.TruncateAsync<T>();
            transaction.Commit();
        }
        catch (Exception ex)
        {
            logger.LogError("Error in truncating values in database ");
            logger.LogError(ex.Message);
        }
    }

    public async Task Update(T entity)
    {
        using AppDbContext appDbConext = await contextFactory.CreateDbContextAsync();
        try
        {
            appDbConext.Update(entity);
            await appDbConext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError("Error in updating values in database ");
            logger.LogError(ex.Message);
        }
    }

    public async Task Update(IEnumerable<T> entities)
    {
        using AppDbContext appDbConext = await contextFactory.CreateDbContextAsync();
        try
        {
            using var transaction = appDbConext.Database.BeginTransaction();
            var entities1 = entities.ToList();
            var bulkConfig = new BulkConfig { SetOutputIdentity = true, BatchSize = 4000 };
            await appDbConext.BulkUpdateAsync(entities1, bulkConfig);
            transaction.Commit();
            //appDbConext.UpdateRange(entities);
            //await appDbConext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError("Error in updating values in database ");
            logger.LogError(ex.Message);
        }
    }

    Task<T?> IRepository<T>.FindById(int id, params Expression<Func<T, object>>[] includeProperties)
    {
        throw new NotImplementedException();
    }
}