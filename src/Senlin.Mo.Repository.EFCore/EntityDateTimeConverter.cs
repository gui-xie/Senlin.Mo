using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Senlin.Mo.Domain;

namespace Senlin.Mo.Repository.EFCore;

/// <summary>
/// EntityDateTime Converter
/// </summary>
public class EntityDateTimeConverter()
    : ValueConverter<EntityDateTime, DateTime>(
        v => v, 
        v=> (EntityDateTime)v);
