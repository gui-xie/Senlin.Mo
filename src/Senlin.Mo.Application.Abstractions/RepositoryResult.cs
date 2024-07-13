using Senlin.Mo.Domain;

namespace Senlin.Mo.Application.Abstractions;

/// <summary>
/// Repository operation result
/// </summary>
public class RepositoryResult
{
    private RepositoryResult(RepositoryResultType type)
    {
        Type = type;
    }

    /// <summary>
    /// Result type
    /// </summary>
    public RepositoryResultType Type { get; }

    /// <summary>
    /// Ok
    /// </summary>
    /// <returns></returns>
    public static RepositoryResult Ok() => new(RepositoryResultType.Ok);

    /// <summary>
    /// Repeat
    /// </summary>
    /// <returns></returns>
    public static RepositoryResult Repeat() => new(RepositoryResultType.Repeat);

    /// <summary>
    /// Convert to bool implicitly
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    public static implicit operator bool(RepositoryResult result) => result.Type == RepositoryResultType.Ok;
}