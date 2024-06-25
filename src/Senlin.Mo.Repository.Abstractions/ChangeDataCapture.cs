namespace Senlin.Mo.Repository.Abstractions;

/// <summary>
/// Change Data Capture
/// </summary>
public class ChangeDataCapture
{
    private ChangeDataCapture()
    {
    }

    /// <summary>
    /// Id
    /// </summary>
    public long Id { get; private set; }

    /// <summary>
    /// EntityId
    /// </summary>
    public long EntityId { get; private set; }

    /// <summary>
    /// Entity Type
    /// </summary>
    public string EntityType { get; private set; } = string.Empty;

    /// <summary>
    /// Change User
    /// </summary>
    public string ChangeUser { get; private set; } = string.Empty;

    /// <summary>
    /// Change Time
    /// </summary>
    public DateTime ChangeTime { get; private set; }

    /// <summary>
    /// Change Type
    /// </summary>
    public string Type { get; private set; }

    /// <summary>
    /// Change Data
    /// </summary>
    public string ChangeData { get; private set; } = string.Empty;

    /// <summary>
    /// Create Change Data Capture
    /// </summary>
    /// <param name="entityId">entity id</param>
    /// <param name="entityType">entity type</param>
    /// <param name="changeUser">change user</param>
    /// <param name="changeTime">change time</param>
    /// <param name="type">type</param>
    /// <param name="changeContent">change content</param>
    /// <returns></returns>
    public static ChangeDataCapture Create(
        long entityId,
        string entityType,
        string changeUser,
        DateTime changeTime,
        string type,
        string changeContent
    ) => new()
    {
        EntityId = entityId,
        EntityType = entityType,
        ChangeUser = changeUser,
        ChangeTime = changeTime,
        Type = type,
        ChangeData = changeContent
    };
}