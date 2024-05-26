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
    protected Task<T?> GetAsync(EntityId id, CancellationToken cancellationToken = default) =>
        EntitySet.FirstOrDefaultAsync(e =>
                EF.Property<EntityId>(e, Id) == id, 
            cancellationToken);

    /// <summary>
    /// Is entity exists
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private Task<bool> IsExistsAsync(IUnique<T> entity, CancellationToken cancellationToken = default) =>
        EntitySet.AnyAsync(entity.IsRepeatExpression, cancellationToken);

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
        if (entity is IUnique<T> uniqueEntity 
            && await EntitySet.AnyAsync(
                uniqueEntity.IsRepeatExpression,
                cancellationToken))
        {
            return Repeat();
        }
        var id = helper.NewId();
        var user = helper.GetUserId();
        var now = helper.GetNow();
        entry.Property(Id).CurrentValue = id;
        entry.Property(CreateUser).CurrentValue = user;
        entry.Property(CreateTime).CurrentValue = now;
        entry.Property(UpdateUser).CurrentValue = string.Empty;
        entry.Property(UpdateTime).CurrentValue = EntityDateTime.Empty;
        entry.Property(DeleteUser).CurrentValue = string.Empty;
        entry.Property(DeleteTime).CurrentValue = EntityDateTime.Empty;
        entry.Property(Tenant).CurrentValue = helper.GetTenant();
        entry.Property(ConcurrencyToken).CurrentValue = helper.NewConcurrencyToken();
        // entry.Property(UniqueToken).CurrentValue = entity.IsUnique()
        dbContext.Add(entity);

        if (!helper.IsContainsChangeDataCapture()) return Ok();
        
        var cdc = CreateCdc(id, user, now, ChangeDataCaptureType.Add, entity);
        var cdcEntry = dbContext.Entry(cdc);
        cdcEntry.Property(ChangeDataCaptureExtensions.MonthName).CurrentValue = ((DateTime)now).ToString("yyMM");
        dbContext.Add(cdc);
        return Ok();
    }

    private static ChangeDataCapture CreateCdc<TEntity>(
        EntityId entityId,
        string user,
        EntityDateTime now,
        ChangeDataCaptureType type,
        TEntity entity
    ) =>
        ChangeDataCapture.Create(
            entityId,
            typeof(T).Name,
            user,
            now,
            type,
            JsonSerializer.Serialize(entity, ChangeDataCaptureExtensions.SerializerOptions));
}