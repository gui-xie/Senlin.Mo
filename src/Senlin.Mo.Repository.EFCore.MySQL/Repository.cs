using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Senlin.Mo.Domain;
using Senlin.Mo.Repository.Abstractions;
using static Senlin.Mo.Repository.EFCore.EntityShadowPropertyNames;

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
    where T: class, IEntity
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
    /// Add entity
    /// </summary>
    /// <param name="entity"></param>
    protected void AddEntity(T entity)
    {
        var entry = EntitySet.Entry(entity);
        var id = helper.NewId();
        var user = helper.GetUserId();
        var now = helper.GetNow();
        var tenant = helper.GetTenant();
        entry.Property(Id).CurrentValue = id;
        entry.Property(CreateUser).CurrentValue = user;
        entry.Property(CreateTime).CurrentValue = now;
        entry.Property(UpdateUser).CurrentValue = string.Empty;
        entry.Property(UpdateTime).CurrentValue = EntityDateTime.Empty;
        entry.Property(DeleteUser).CurrentValue = string.Empty;
        entry.Property(DeleteTime).CurrentValue = EntityDateTime.Empty;
        dbContext.Add(entity);

        if (!helper.IsContainsChangeDataCapture()) return;
        var cdc = CreateCdc(id, user, now, ChangeDataCaptureType.Add, entity);
        dbContext.Add(cdc);
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
            JsonSerializer.Serialize(entity));
}