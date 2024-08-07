﻿using Senlin.Mo.Application.Abstractions;

namespace Senlin.Mo;

public class MoConfigureOptions
{
    /// <summary>
    /// Logger options
    /// </summary>
    public LoggerOptions Logger { get; set; } = new();

    /// <summary>
    /// Localization options
    /// </summary>
    public LocalizationOptions LocalizationOptions { get; set; } = new();
    
    /// <summary>
    /// System tenant
    /// </summary>
    public string SystemTenant { get; set; } = "__";
}
