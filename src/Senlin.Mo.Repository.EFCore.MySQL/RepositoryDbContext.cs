using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Senlin.Mo.Application.Abstractions;
using Senlin.Mo.Domain;
using Senlin.Mo.Repository.Abstractions;
using static Senlin.Mo.Repository.EFCore.EntityShadowPropertyNames;

namespace Senlin.Mo.Repository.EFCore.MySQL;

/// <summary>
/// Repository DbContext
/// </summary>
/// <param name="connectionString"></param>
/// <param name="helper"></param>
/// <typeparam name="T"></typeparam>
public abstract class RepositoryDbContext<T>(
    ConnectionString<T> connectionString,
    IRepositoryHelper helper)
    : DbContext, IRepositoryDbContext, IUnitOfWorkHandler
    where T : RepositoryDbContext<T>, IUnitOfWorkHandler
{
    /// <summary>
    /// Configure
    /// </summary>
    /// <param name="optionsBuilder"></param>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseMySql(
            connectionString,
            ServerVersion.AutoDetect(connectionString),
            options => options.UseMicrosoftJson());
        optionsBuilder.EnableSensitiveDataLogging();
        optionsBuilder.UseSnakeCaseNamingConvention();
    }

    /// <summary>
    /// Configure conventions
    /// </summary>
    /// <param name="configurationBuilder"></param>
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
    }

    /// <summary>
    /// Get domain assembly
    /// </summary>
    /// <returns></returns>
    protected abstract Assembly GetDomainAssembly();

    /// <summary>
    /// Configure model
    /// </summary>
    /// <param name="modelBuilder"></param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("utf8mb4_zh_0900_as_cs").HasCharSet("utf8mb4");

        var changeDataCaptureBuilder = modelBuilder.Entity<ChangeDataCapture>();
        changeDataCaptureBuilder.Property<string>(ChangeDataCaptureExtensions.MonthName);
        changeDataCaptureBuilder.Property(e => e.ChangeData).HasColumnType("json");
        changeDataCaptureBuilder.HasKey(e => e.Id);

        var assembly = typeof(T).Assembly;
        modelBuilder.ApplyConfigurationsFromAssembly(assembly);
        var entityTypes = from type in GetDomainAssembly().ExportedTypes
            where type.IsClass && !type.IsAbstract && type.IsAssignableTo(typeof(Entity))
            select type;
        foreach (var entityType in entityTypes)
        {
            typeof(T).BaseType!
                .GetMethod(
                    nameof(ApplyEntityModel),
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod)!
                .MakeGenericMethod(entityType)
                .Invoke(this, [modelBuilder]);
        }
    }

    private void ApplyEntityModel<TEntity>(ModelBuilder modelBuilder) where TEntity : Entity
    {
        var builder = modelBuilder.Entity<TEntity>();

        builder.HasQueryFilter(e =>
            !EF.Property<bool>(e, IsDelete) &&
            (helper.IsSystemTenant() ||
             helper.GetTenant() == EF.Property<string>(e, Tenant)));

        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property<string>(Tenant);
        builder.Property<byte[]>(ConcurrencyToken).IsConcurrencyToken();
        builder.Property<string>(CreateUser);
        builder.Property<string>(UpdateUser);
        builder.Property<string>(DeleteUser);
        builder.Property<DateTime>(CreateTime);
        builder.Property<DateTime>(UpdateTime);
        builder.Property<DateTime>(DeleteTime);
        builder.Property<bool>(IsDelete);

        builder.HasKey(e => e.Id);
    }

    /// <summary>
    /// Get domain events
    /// </summary>
    /// <param name="dbContext"></param>
    /// <returns></returns>
    public List<IDomainEvent> GetDomainEvents(DbContext dbContext)
    {
        var entries = dbContext.ChangeTracker.Entries<Entity>();
        var domainEvents = entries.SelectMany(e => e.Entity.GetDomainEvents());
        return domainEvents.ToList();
    }

    public new Task SaveChangesAsync(CancellationToken cancellationToken)
        => base.SaveChangesAsync(cancellationToken);
}