using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Senlin.Mo.Domain;

namespace Senlin.Mo.Repository.EFCore;

/// <summary>
/// EntityId Converter
/// </summary>
public class EntityIdConverter()
    : ValueConverter<EntityId, long>(
        v => v,
        v => (EntityId)v);