namespace Senlin.Mo.Domain;

/// <summary>
/// Domain Extensions
/// </summary>
public static class DomainExtensions
{
    /// <summary>
    /// Id
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static long Id(this string value) => Convert.ToInt64(value);

    /// <summary>
    /// Is Empty Id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static bool IsEmptyId(this long id) => id == 0;

    /// <summary>
    /// Empty DateTime
    /// </summary>
    public static readonly DateTime EmptyDateTime = DateTimeOffset.FromUnixTimeSeconds(0).UtcDateTime;
}