namespace Senlin.Mo.Application.Abstractions.Decorators.UnitOfWork;

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
    public bool IsEnable { get; set; } = true;

    /// <summary>
    /// Decorator Service Type
    /// </summary>
    public Type ServiceType => typeof(UnitOfWorkService<,>);

    /// <summary>
    /// Configure
    /// </summary>
    /// <param name="service"></param>
    public void Configure(IService? service)
    {
        if(service is IUnitOfWorkService unitOfWorkService)
        {
            unitOfWorkService.IsEnable = IsEnable;
        }
    }
}