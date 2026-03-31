using System.Linq.Expressions;

namespace TravelNow.Application.Interfaces.Repositories;

public interface IBaseRepository<T> where T: class
{
    //Get
    Task<T?> GetAsync(Guid Id);

    Task<T?> GetFirstAsync(Expression<Func<T, bool>> predicate);

    Task<List<T>?> GetAllAsync(Expression<Func<T, bool>>? predicate = null, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null);

    //Add
    Task AddAsync(T entity);

    Task AddRangeAsync(IEnumerable<T> entities);

    Task AddRangeAsync(params T[] entities);


    //Update
    Task UpdateAsync(T entity);

    Task UpdateRangeAsync(IEnumerable<T> entities);

    Task UpdateRangeAsync(params T[] entities);

    //Delete
    Task DeleteAsync(T entity);

    Task DeleteRangeAsync(IEnumerable<T> entities);

    Task DeleteRangeAsync(params T[] entities);
}
