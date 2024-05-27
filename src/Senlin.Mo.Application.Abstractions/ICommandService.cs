using Senlin.Mo.Domain;

namespace Senlin.Mo.Application.Abstractions;

/// <summary>
/// Command service interface
/// </summary>
/// <typeparam name="TCommand">Command</typeparam>
public interface ICommandService<in TCommand> : IService<TCommand, Result>
{
    
}

/// <summary>
/// Command service interface with response data
/// </summary>
/// <typeparam name="TCommand">Command</typeparam>
/// <typeparam name="TResultData">Result data</typeparam>
public interface ICommandService<in TCommand, TResultData> : IService<TCommand, Result<TResultData>>
{
    
}