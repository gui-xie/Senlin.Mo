﻿using Microsoft.Extensions.DependencyInjection;

namespace Senlin.Mo.Application.Abstractions;

/// <summary>
/// Service registration
/// </summary>
public class ServiceRegistration
{
    /// <summary>
    /// Service registration
    /// </summary>
    /// <param name="serviceType"></param>
    /// <param name="implementation"></param>
    /// <param name="decorators"></param>
    /// <param name="lifeCycle"></param>
    /// <param name="routeData"></param>
    public ServiceRegistration(
        Type serviceType,
        Type implementation,
        Type[]? decorators = null,
        ServiceLifetime lifeCycle = ServiceLifetime.Transient,
        ServiceRouteData? routeData = null)
    {
        ServiceType = serviceType;
        Implementation = implementation;
        Decorators = decorators;
        LifeCycle = lifeCycle;
        RouteData = routeData;
    }

    /// <summary>
    /// Service Type
    /// </summary>
    public Type ServiceType { get; }

    /// <summary>
    /// Service Implementation
    /// </summary>
    public Type Implementation { get; }

    /// <summary>
    /// Service Decorator
    /// </summary>
    public Type[]? Decorators { get; }

    /// <summary>
    /// Service LifeCycle
    /// </summary>
    public ServiceLifetime LifeCycle { get; }

    /// <summary>
    /// Route Pattern, Handler, Methods
    /// </summary>
    public ServiceRouteData? RouteData { get; }
}