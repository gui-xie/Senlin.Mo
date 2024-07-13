namespace Senlin.Mo.Domain;


/// <summary>
/// Result Interface
/// </summary>
public interface IResult
{
    /// <summary>
    /// Is Success
    /// </summary>
    bool IsSuccess { get; }

    /// <summary>
    /// Get Error Message
    /// </summary>
    /// <returns></returns>
    string GetErrorMessage();
}

/// <summary>
/// Result with T data
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IResult<out T>: IResult
{
    /// <summary>
    /// Get Data
    /// </summary>
    /// <returns></returns>
    T? GetData();
}