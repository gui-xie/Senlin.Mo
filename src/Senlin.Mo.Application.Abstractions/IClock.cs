namespace Senlin.Mo.Application.Abstractions;

/// <summary>
/// Clock
/// </summary>
public interface IClock
{
    /// <summary>
    /// Get current time stamp
    /// </summary>
    GetNow GetNow { get; }
}