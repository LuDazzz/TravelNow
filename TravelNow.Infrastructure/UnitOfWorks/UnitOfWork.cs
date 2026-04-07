using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using TravelNow.Application.Interfaces.Repositories;
using TravelNow.Application.Interfaces.UnitOfWorks;
using TravelNow.Domain.Entities.Base;
using TravelNow.Infrastructure.Repositories;
using TravelNow.Shared.Helper;

namespace TravelNow.Infrastructure.UnitOfWorks;

public class UnitOfWork : IUnitOfWork
{
    private readonly TravelNowDbContext _context;
    private readonly Dictionary<Type, object> _repositories;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UnitOfWork(
        TravelNowDbContext context,
        IHttpContextAccessor httpContextAccessor
        )
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _httpContextAccessor = httpContextAccessor;
        _repositories = new Dictionary<Type, object>();
        DbContext = _context;
    }

    public DbContext DbContext { get; }

    public void Dispose()  
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    public IBaseRepository<T> GetRepository<T>() where T : class
    {
        var type = typeof(BaseRepository<T>);

        if (!_repositories.TryGetValue(type, out var value))
        {
            value = new BaseRepository<T>(_context);
            _repositories[type] = value;
        }

        return (IBaseRepository<T>)value;
    }

    public async Task<int> SaveChanges()
    {
        SaveChangesInternal();

        return await _context.SaveChangesAsync();
    }

    private void SaveChangesInternal()
    {
        var entries = _context.ChangeTracker.Entries()
            .Where(x => x.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToArray();
        if (entries.Length == 0) return;

        SaveChangesInternal(entries, EntityState.Added);
        SaveChangesInternal(entries, EntityState.Modified);

        var deletedEntries = _context.ChangeTracker.Entries()
            .Where(x => x.State == EntityState.Deleted);
        SaveChangesSoftDelete(deletedEntries);
    }

    private void SaveChangesInternal(EntityEntry[] entries, EntityState state)
    {
        // Enforce type defaults for all entities
        foreach (var item in entries)
            foreach (var p in item.Properties)
            {
                if (p.CurrentValue == null) continue;

                switch (p.Metadata.ClrType.Name)
                {
                    case "String": // Replace all empty strings with null
                        var emptyString = string.IsNullOrWhiteSpace(p.CurrentValue.ToString());
                        p.CurrentValue = emptyString ? null : p.CurrentValue;
                        break;
                }
            }

        foreach (var item in entries.Where(t => t.State == state))
        {
            var now = DateTimeHelper.GetDtOffsetUtc();
            PropertyEntry? propertyEntry;
            if (state == EntityState.Added)
            {
                // CreatedBy
                propertyEntry = item.Properties.FirstOrDefault(p => p.Metadata.Name == "CreatedBy");
                if (propertyEntry != null)
                    if (_httpContextAccessor?.HttpContext?.User != null
                        && (Guid?)propertyEntry.CurrentValue == Guid.Empty)
                        propertyEntry.CurrentValue = _httpContextAccessor.HttpContext.User.Claims.GetUserIdNullable();

                // CreatedAt
                propertyEntry = item.Properties.FirstOrDefault(p => p.Metadata.Name == "CreatedAt");
                if (propertyEntry != null) propertyEntry.CurrentValue = now;
            }

            // UpdatedBy
            propertyEntry = item.Properties.FirstOrDefault(p => p.Metadata.Name == "UpdatedBy");
            if (propertyEntry != null)
                if (_httpContextAccessor?.HttpContext?.User != null
                    && (Guid?)propertyEntry.CurrentValue == Guid.Empty)
                    propertyEntry.CurrentValue = _httpContextAccessor.HttpContext.User.Claims.GetUserIdNullable();

            // UpdatedAt
            propertyEntry = item.Properties.FirstOrDefault(p => p.Metadata.Name == "UpdatedAt");
            if (propertyEntry != null) propertyEntry.CurrentValue = now;

            // Trim String Entries Before Saving
            var propertyValues = item.Properties
                .Where(p => p.CurrentValue is string && !string.IsNullOrEmpty(Convert.ToString(p.CurrentValue)));
            foreach (var propertyValue in propertyValues)
                propertyValue.CurrentValue = (propertyValue.CurrentValue?.ToString() ?? string.Empty).Trim();
        }
    }

    private void SaveChangesSoftDelete(IEnumerable<EntityEntry> entries)
    {
        foreach (var item in entries)
        {
            if (item.Entity is not IIsDeletedEntity entity) continue;

            // Set the entity to unchanged (if we mark the whole entity as Modified, every field gets sent to Db as an update)
            item.State = EntityState.Unchanged;

            // Only update the IsDeleted flag - only this will get sent to the Db
            entity.IsDeleted = true;

            if (item.Entity is not IAuditEntity auditEntity) continue;

            if (_httpContextAccessor?.HttpContext?.User != null)
                auditEntity.UpdatedBy = _httpContextAccessor.HttpContext.User.Claims.GetUserIdNullable();

            auditEntity.UpdatedAt = DateTimeHelper.GetDtOffsetUtc();
        }
    }
}
