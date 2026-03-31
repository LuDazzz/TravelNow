using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TravelNow.Application.Interfaces.Repositories;

namespace TravelNow.Infrastructure.Repositories;

public class BaseRepository<T>(TravelNowDbContext context) : IBaseRepository<T> where T : class
{
    protected DbSet<T> DbSet { get; } = context.Set<T>();

    //Add
    public async Task AddAsync(T entity)
    {
        await DbSet.AddAsync(entity);
    }

    public async Task AddRangeAsync(IEnumerable<T> entities)
    {
        await DbSet.AddRangeAsync(entities);
    }

    public async Task AddRangeAsync(params T[] entities)
    {
        await DbSet.AddRangeAsync(entities);
    }

    //Delete
    public async Task DeleteAsync(T entity)
    {
        DbSet.Remove(entity);
        await Task.CompletedTask;
    }

    public async Task DeleteRangeAsync(IEnumerable<T> entities)
    {
        DbSet.RemoveRange(entities);
        await Task.CompletedTask;
    }

    public async Task DeleteRangeAsync(params T[] entities)
    {
        DbSet.RemoveRange(entities);
        await Task.CompletedTask;
    }

    //Get
    public async Task<List<T>?> GetAllAsync(Expression<Func<T, bool>>? predicate = null, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null)
    {
        var queryable = DbSet.AsQueryable();
        if (predicate != null)
        {
            queryable = queryable.Where(predicate);
        }

        if (orderBy != null)
        {
            queryable = orderBy(queryable);
        }

        return await queryable.ToListAsync();
    }

    public async Task<T?> GetAsync(Guid Id)
    {
        return await DbSet.FindAsync(Id);
    }

    public async Task<T?> GetFirstAsync(Expression<Func<T, bool>> predicate)
    {
        return await DbSet.Where(predicate).FirstOrDefaultAsync();
    }

    //Update
    public async Task UpdateAsync(T entity)
    {
        DbSet.Update(entity);
        await Task.CompletedTask;
    }

    public async Task UpdateRangeAsync(IEnumerable<T> entities)
    {
        DbSet.UpdateRange(entities);
        await Task.CompletedTask;
    }

    public async Task UpdateRangeAsync(params T[] entities)
    {
        DbSet.UpdateRange(entities);
        await Task.CompletedTask;
    }
}
