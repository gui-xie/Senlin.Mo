namespace Senlin.Mo.Domain;

/// <summary>
/// Result
/// </summary>
public class Result
{
    private Result()
    {
        
    }
    
    /// <summary>
    /// Result Type
    /// </summary>
    public ResultStatus Status { get; private set; }

    /// <summary>
    /// Error Message
    /// </summary>
    public string Message { get; private set; } = string.Empty;
    
    /// <summary>
    /// Data
    /// </summary>
    public object? Data { get; private set; }
    
    /// <summary>
    /// Create Success Result
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static Result Success(object? data = null) =>
        new()
        {
            Status = ResultStatus.Success,
            Data = data
        };
    
    /// <summary>
    /// Create Error Result
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public static Result Fail(string message) =>
        new()
        {
            Status = ResultStatus.Fail,
            Message = message
        };

    /// <summary>
    /// Convert to bool
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    public static implicit operator bool(Result result) => result.Status == ResultStatus.Success;

    /// <summary>
    /// Convert to Task
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    public static implicit operator Task<Result>(Result result) => Task.FromResult(result);
}