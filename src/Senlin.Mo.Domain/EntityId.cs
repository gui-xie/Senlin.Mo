namespace Senlin.Mo.Domain;

/// <summary>
/// Entity Id
/// </summary>
public readonly struct EntityId : IEquatable<EntityId>
{
    private static readonly long EmptyEntityId = 0;

    private readonly long _id;

    /// <summary>
    ///  EntityId
    /// </summary>
    /// <param name="id">string id</param>
    public EntityId(string? id)
    {
        _id = string.IsNullOrWhiteSpace(id)
            ? EmptyEntityId
            : Convert.ToInt64(id);
    }

    /// <summary>
    /// EntityId
    /// </summary>
    /// <param name="id">long id</param>
    public EntityId(long id)
    {
        _id = id;
    }

    /// <summary>
    /// Convert to string Implicitly
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static implicit operator long(EntityId id) => id._id == EmptyEntityId ? 0 : id._id;

    /// <summary>
    /// Convert to EntityId Implicitly
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static implicit operator string(EntityId id) => id._id == EmptyEntityId ? string.Empty : id._id.ToString();

    /// <summary>
    /// Convert to EntityId from long Explicitly
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static explicit operator EntityId(long id) => new(id);

    /// <summary>
    /// Convert to EntityId from string Explicitly
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static explicit operator EntityId(string? id) => new(id);

    /// <summary>
    /// Is equal compare to another EntityId 
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static bool operator ==(EntityId left, EntityId right) => left.Equals(right);

    /// <summary>
    /// Is not equal compare to another EntityId
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static bool operator !=(EntityId left, EntityId right) => !(left == right);

    /// <summary>
    /// Is equal compare to another EntityId
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool Equals(EntityId other) => _id == other._id;

    /// <summary>
    /// Is equal compare to another object
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool Equals(object? obj) => obj is EntityId other && Equals(other);

    /// <summary>
    /// Get hash code
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode() => _id.GetHashCode();

    /// <summary>
    /// To string
    /// </summary>
    /// <returns></returns>
    public override string ToString() => _id == 0 ? string.Empty : _id.ToString();
}