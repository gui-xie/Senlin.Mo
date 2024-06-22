namespace Senlin.Mo.Application.Abstractions;

/// <summary>
/// Unit Of Work Attribute
/// </summary>
public class UnitOfWorkAttribute(bool isEnable = true) : Attribute
{
    /// <summary>
    /// Is enable
    /// </summary>
    public bool IsEnable { get; } = isEnable;
}
