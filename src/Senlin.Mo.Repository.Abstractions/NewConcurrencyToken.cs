using Senlin.Mo.Domain;

namespace Senlin.Mo.Repository.Abstractions;

/// <summary>
/// generate a new concurrency token
/// </summary>
public delegate EntityId NewConcurrencyToken();