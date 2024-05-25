namespace Senlin.Mo.Application.Abstractions;

/// <summary>
/// Clock
/// </summary>
public interface IClock
{
    /// <summary>
    /// Get current utc time
    /// </summary>
    GetNow GetNow { get; }
}