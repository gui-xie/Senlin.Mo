﻿using Senlin.Mo.Application.Abstractions;

namespace Senlin.Mo.Module;

/// <summary>
/// Module Interface
/// </summary>
public interface IModule
{
    /// <summary>
    /// Module Name
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Get Services registration
    /// </summary>
    /// <returns></returns>
    IEnumerable<ServiceRegistration> GetServices();

    /// <summary>
    /// Get Repositories registration
    /// </summary>
    /// <returns></returns>
    IEnumerable<(Type Abstraction, Type Implementation)> GetRepositories();

    /// <summary>
    /// Get DbContext Type
    /// </summary>
    Type DbContextType { get; }

    /// <summary>
    /// Get Localization Resource Resolver Types
    /// </summary>
    Type LStringResolverType { get; }
}