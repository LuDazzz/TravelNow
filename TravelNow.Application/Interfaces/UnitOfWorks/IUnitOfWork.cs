using Microsoft.EntityFrameworkCore;
using TravelNow.Application.Interfaces.Repositories;

namespace TravelNow.Application.Interfaces.UnitOfWorks;

public interface IUnitOfWork : IDisposable
{
    DbContext DbContext { get; }

    IBaseRepository<T> GetRepository<T>() where T : class;

    Task<int> SaveChanges();
}
