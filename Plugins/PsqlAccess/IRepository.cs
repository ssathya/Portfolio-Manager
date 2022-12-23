using ApplicationModels;
using System.Linq.Expressions;

namespace PsqlAccess;

public interface IRepository<T> where T : Entity
{
    Task<T?> FindById(int id, params Expression<Func<T, object>>[] includeProperties);

    Task<IEnumerable<T>> FindAll(params Expression<Func<T, object>>[] includeProperties);

    Task<IEnumerable<T>> FindAll(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includeProperties);

    Task Add(T entity);

    Task Add(IEnumerable<T> entities);

    Task Remove(T entity);

    Task Remove(IEnumerable<T> entities);

    Task Remove(int id);

    Task Truncate();

    Task Update(T entity);

    Task Update(IEnumerable<T> entities);
}