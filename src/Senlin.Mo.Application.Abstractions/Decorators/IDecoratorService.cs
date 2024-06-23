namespace Senlin.Mo.Application.Abstractions.Decorators;

/// <summary>
/// Decorator Service
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IDecoratorService<T> where T : IServiceDecorator
{
    /// <summary>
    /// Decorator Attribute Data (Injected by DI container)
    /// </summary>
    T AttributeData { get; set; }
}