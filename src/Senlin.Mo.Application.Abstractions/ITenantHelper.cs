namespace Senlin.Mo.Application.Abstractions;

/// <summary>
/// Get current tenant id
/// </summary>
public interface ITenantHelper
{
    /// <summary>
    /// Get current tenant id
    /// </summary>
    GetTenant GetTenant { get; }

    /// <summary>
    /// Is filter tenant
    /// </summary>
    bool IsFilterTenant();
}