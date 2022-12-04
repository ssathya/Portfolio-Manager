using ApplicationModels;
using System.Linq.Expressions;

namespace PsqlAccess;

public interface IRepository<T> where T : IEntity
{
    T FindById(int id, params Expression<Func<T, object>>[] includeProperties);

    IQueryable<T> FindAll(params Expression<Func<T, object>>[] includeProperties);

    IEnumerable<T> FindAll(Expression<Func<T, object>> predicate, params Expression<Func<T, object>>[] includeProperties);

    void Add(T entity);

    void Remove(T entity);

    void Remove(int id);

    void Update(T entity);
}