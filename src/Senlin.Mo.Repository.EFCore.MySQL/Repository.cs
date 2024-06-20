using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Senlin.Mo.Application.Abstractions;
using Senlin.Mo.Domain;
using Senlin.Mo.Repository.Abstractions;
using static Senlin.Mo.Repository.EFCore.EntityShadowPropertyNames;
using static Senlin.Mo.Application.Abstractions.RepositoryResult;

namespace Senlin.Mo.Repository.EFCore.MySQL;

/// <summary>
/// Repository base class
/// </summary>
/// <param name="dbContext"></param>
/// <param name="helper"></param>
/// <typeparam name="T"></typeparam>
public abstract class Repository<T>(
    DbContext dbContext,
    IRepositoryHelper helper)
    where T: Entity
{
    private DbSet<T> EntitySet => dbContext.Set<T>();

    /// <summary>
    /// Get entity by id
    /// </summary>
    /// <param name="id">id</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected Task<T?> GetAsync(long id, CancellationToken cancellationToken = default) =>
        EntitySet.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    /// <summary>
    /// Delete entity
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="cancellationToken"></param>
    protected void DeleteEntity(
        T entity,
        CancellationToken cancellationToken = default)
    {
        var entry = EntitySet.Entry(entity);
        var user = helper.GetUserId();
        var now = helper.GetNow();
        entry.Property(DeleteUser).CurrentValue = user;
        entry.Property(DeleteTime).CurrentValue = now;
        entry.Property(IsDelete).CurrentValue = true;
        entry.Property(ConcurrencyToken).CurrentValue = helper.NewConcurrencyToken();
        dbContext.Update(entity);

        if (!helper.IsContainsChangeDataCapture()) return;
        
        var cdc = CreateCdc(entity.Id, user, now, ChangeDataCaptureType.Delete, entity);
        dbContext.Add(cdc);
    }
    
    /// <summary>
    /// Update entity
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="cancellationToken"></param>
    protected void UpdateEntity(
        T entity,
        CancellationToken cancellationToken = default)
    {
        var entry = EntitySet.Entry(entity);
        var user = helper.GetUserId();
        var now = helper.GetNow();
        entry.Property(UpdateUser).CurrentValue = user;
        entry.Property(UpdateTime).CurrentValue = now;
        entry.Property(ConcurrencyToken).CurrentValue = helper.NewConcurrencyToken();
        dbContext.Update(entity);

        if (!helper.IsContainsChangeDataCapture()) return;
        
        var cdc = CreateCdc(entity.Id, user, now, ChangeDataCaptureType.Update, entity);
        dbContext.Add(cdc);
    }

    /// <summary>
    /// Add entity
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="cancellationToken"></param>
    protected async Task<RepositoryResult> AddEntityAsync(
        T entity,
        CancellationToken cancellationToken = default)
    {
        var entry = EntitySet.Entry(entity);
        // ReSharper disable once SuspiciousTypeConversion.Global, will be used in other project
        if (entity is IUnique<T> uniqueEntity)
        {
            var expression = uniqueEntity.GetIsRepeatExpression();
            if (await EntitySet.AnyAsync(expression, cancellationToken))
            {
                return Repeat();
            }
        }
        var id = helper.NewId();
        var user = helper.GetUserId();
        var now = helper.GetNow();
        entry.Property<long>(nameof(Entity.Id)).CurrentValue = id;
        entry.Property(CreateUser).CurrentValue = user;
        entry.Property(CreateTime).CurrentValue = now;
        entry.Property(UpdateUser).CurrentValue = string.Empty;
        entry.Property(UpdateTime).CurrentValue = 0;
        entry.Property(DeleteUser).CurrentValue = string.Empty;
        entry.Property(DeleteTime).CurrentValue = 0;
        entry.Property(Tenant).CurrentValue = helper.GetTenant();
        entry.Property(ConcurrencyToken).CurrentValue = helper.NewConcurrencyToken();
        // entry.Property(UniqueToken).CurrentValue = entity.IsUnique()
        dbContext.Add(entity);

        if (!helper.IsContainsChangeDataCapture()) return Ok();
        
        var cdc = CreateCdc(id, user, now, ChangeDataCaptureType.Add, entity);
        dbContext.Add(cdc);
        return Ok();
    }

    private ChangeDataCapture CreateCdc<TEntity>(
        long entityId,
        string user,
        long now,
        ChangeDataCaptureType type,
        TEntity entity
    )
    {
        var cdc = ChangeDataCapture.Create(
            entityId,
            typeof(T).Name,
            user,
            now,
            type,
            JsonSerializer.Serialize(entity, ChangeDataCaptureExtensions.SerializerOptions));
        var cdcEntry = dbContext.Entry(cdc);
        cdcEntry.Property(ChangeDataCaptureExtensions.MonthName).CurrentValue = DateTimeOffset.FromUnixTimeSeconds(now).ToLocalTime().ToString("yyMM");
        return cdc;
    }
}