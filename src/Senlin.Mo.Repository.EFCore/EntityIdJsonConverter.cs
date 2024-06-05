using System.Text.Json;
using System.Text.Json.Serialization;
using Senlin.Mo.Domain;

namespace Senlin.Mo.Repository.EFCore;

/// <summary>
/// EntityId Json Converter
/// </summary>
public class EntityIdJsonConverter : JsonConverter<EntityId>
{
    /// <summary>
    /// Read
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="typeToConvert"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public override EntityId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) 
        => new(reader.GetString());

    /// <summary>
    /// Write
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="value"></param>
    /// <param name="options"></param>
    public override void Write(Utf8JsonWriter writer, EntityId value, JsonSerializerOptions options) 
        => writer.WriteStringValue(value.ToString());
}