using System.Text.Json;
using System.Text.Json.Serialization;
using Senlin.Mo.Domain;

namespace Senlin.Mo.Repository.EFCore.MySQL;

/// <summary>
/// ChangeDataCaptureExtensions
/// </summary>
public static class ChangeDataCaptureExtensions
{
    public const int MaxTypeLength = 10;

    internal const string MonthName = "Month";
    
    internal static readonly JsonSerializerOptions SerializerOptions = new()
    {
        IgnoreReadOnlyFields = true
    };
}
