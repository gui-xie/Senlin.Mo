using System.Text.Json;

namespace Senlin.Mo.Repository.EFCore.MySQL;

/// <summary>
/// ChangeDataCaptureExtensions
/// </summary>
public static class ChangeDataCaptureExtensions
{
    internal const string MonthName = "Month";
    
    internal static readonly JsonSerializerOptions SerializerOptions = new()
    {
        IgnoreReadOnlyProperties = true,
        IgnoreReadOnlyFields = true
    };
}