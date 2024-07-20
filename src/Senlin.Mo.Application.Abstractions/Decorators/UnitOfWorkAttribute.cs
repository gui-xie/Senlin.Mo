namespace Senlin.Mo.Application.Abstractions.Decorators;

/// <summary>
/// Unit of work attribute
/// </summary>
public class UnitOfWorkAttribute : Attribute, IServiceDecorator
{
    /// <summary>
    /// UnitOfWork Constructor
    /// </summary>
    /// <param name="isEnable"></param>
    public UnitOfWorkAttribute(bool isEnable = true) => IsEnable = isEnable;

    /// <summary>
    /// Is enable
    /// </summary>
    public bool IsEnable { get; set; }
}