﻿namespace Senlin.Mo.Localization.Abstractions;

/// <summary>
/// Localization string
/// </summary>
/// <param name="key">the key of json(auto generated by json)</param>
/// <param name="defaultValue">the default value of key</param>
/// <param name="args">the args of json value</param>
public readonly struct LString(
    string key,
    string defaultValue,
    params KeyValuePair<string, string>[] args)
{
    /// <summary>
    /// Empty LString
    /// </summary>
    public static LString Empty = new(string.Empty, string.Empty);
    
    /// <summary>
    /// Get the final string by resolve function
    /// </summary>
    /// <param name="resolve"></param>
    /// <returns></returns>
    public string Resolve(Func<string, string>? resolve = null)
    {
        var s = resolve?.Invoke(key);
        if (string.IsNullOrWhiteSpace(s)) s = defaultValue;
        foreach (var arg in args)
        {
            var argKey = arg.Key;
            s = s.Replace($"{{{argKey}}}", arg.Value);
        }

        return s;
    }
}