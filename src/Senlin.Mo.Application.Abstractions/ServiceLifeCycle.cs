namespace Senlin.Mo.Application.Abstractions;

/// <summary>
/// Service Life Cycle
/// </summary>
public enum ServiceLifeCycle{
    /// <summary>
    /// Singleton
    /// </summary>
    Singleton = 0,
    /// <summary>
    /// Transient
    /// </summary>
    Transient = 1,
    /// <summary>
    /// Scoped
    /// </summary>
    Scoped = 2
}