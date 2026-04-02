using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TravelNow.Domain.Entities.Base;
using TravelNow.Domain.Entities.Identity;

namespace TravelNow.Infrastructure;

public class TravelNowDbContext : IdentityDbContext<User, Role, Guid, UserClaim, UserRole, UserLogin, RoleClaim, UserToken>
{
    public TravelNowDbContext(DbContextOptions<TravelNowDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // CRITICAL: Must call base to create Identity schema
        base.OnModelCreating(builder);

        // Apply all entity configurations from this assembly
        builder.ApplyConfigurationsFromAssembly(typeof(TravelNowDbContext).Assembly);

        // Global query filter for soft delete
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (typeof(IIsDeletedEntity).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");
                var property = System.Linq.Expressions.Expression.Property(parameter, nameof(IIsDeletedEntity.IsDeleted));
                var falseConstant = System.Linq.Expressions.Expression.Constant(false);
                var condition = System.Linq.Expressions.Expression.Equal(property, falseConstant);
                var lambda = System.Linq.Expressions.Expression.Lambda(condition, parameter);

                builder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }
    }
}
