
namespace Senlin.Mo.Domain;

/// <summary>
/// Customize Entity Date time
/// </summary>
/// <param name="utcTime"></param>
public readonly struct EntityDateTime(DateTime? utcTime)
{
    private static readonly DateTime EmptyDateTime = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private readonly DateTime _time = utcTime ?? EmptyDateTime;

    /// <summary>
    /// Empty Entity Datetime
    /// </summary>
    public static EntityDateTime Empty => new(EmptyDateTime);

    private DateTime Time => new DateTimeOffset(_time, TimeSpan.Zero).UtcDateTime;

    /// <summary>
    /// Convert to DateTime Implicitly
    /// </summary>
    /// <param name="d"></param>
    /// <returns></returns>
    public static implicit operator DateTime(EntityDateTime d) => d._time;
    
    /// <summary>
    /// Convert to EntityDateTime Implicitly
    /// </summary>
    /// <param name="d"></param>
    /// <returns></returns>
    public static explicit operator EntityDateTime(DateTime d) => new(d);
}