namespace Senlin.Mo.Application.Abstractions.Decorators.Log;

/// <summary>
/// Log attribute
/// </summary>
public class LogAttribute : Attribute, IServiceDecorator
{
    /// <summary>
    /// Log Constructor
    /// </summary>
    /// <param name="isEnable"></param>
    public LogAttribute(bool isEnable = true)
    {
        IsEnable = isEnable;
    }

    /// <summary>
    /// Is enable
    /// </summary>
    public bool IsEnable { get; set; }
}