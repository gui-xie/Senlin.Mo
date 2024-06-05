using System.Text.Json;
using System.Text.Json.Serialization;
using Senlin.Mo.Domain;

namespace Senlin.Mo.Repository.EFCore;

/// <summary>
/// EntityDateTime Json Converter
/// </summary>
public class EntityDateTimeJsonConverter : JsonConverter<EntityDateTime>
{
    /// <summary>
    /// Read
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="typeToConvert"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public override EntityDateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return DateTime.TryParse(reader.GetString(), out var dateTime)
            ? new EntityDateTime(dateTime)
            : new EntityDateTime();
    }

    /// <summary>
    /// Write
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="value"></param>
    /// <param name="options"></param>
    public override void Write(Utf8JsonWriter writer, EntityDateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}