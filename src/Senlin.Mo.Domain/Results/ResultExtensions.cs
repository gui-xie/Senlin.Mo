namespace Senlin.Mo.Domain;

/// <summary>
/// Result Extensions
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Is Result Success
    /// </summary>
    /// <param name="r"></param>
    /// <returns></returns>
    public static bool IsSuccess(this IResult r) => r.Status == ResultStatus.Success;
}