namespace Senlin.Mo.Application.Abstractions;

/// <summary>
/// Get current user id
/// </summary>
public interface IUserHelper
{
    /// <summary>
    /// Get current user id
    /// </summary>
    GetUserId GetUserId { get; }
}