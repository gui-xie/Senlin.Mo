namespace Senlin.Mo.Domain;

/// <summary>
/// Result with T data
/// </summary>
/// <typeparam name="T"></typeparam>
public class Result<T>
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
    public T? Data { get; private set; }
    
    /// <summary>
    /// Create Success Result
    /// </summary>
    /// <returns></returns>
    public static Result<T> Success(T data) =>
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
    public static Result<T> Fail(string message) =>
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
    public static implicit operator bool(Result<T> result) => result.Status == ResultStatus.Success;

    /// <summary>
    /// Convert to Task
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    public static implicit operator Task<Result<T>>(Result<T> result) => Task.FromResult(result);

    /// <summary>
    /// Convert to Result
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    public static implicit operator Result(Result<T> result) =>
        result.Status == ResultStatus.Success ? Result.Success() : Result.Fail(result.Message);
}

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
    /// Create Success Result
    /// </summary>
    /// <returns></returns>
    public static Result Success() =>
        new()
        {
            Status = ResultStatus.Success
        };

    /// <summary>
    /// Create Success Result with data type
    /// </summary>
    /// <returns></returns>
    public static Result<T> Success<T>(T data) => Result<T>.Success(data);
    
    /// <summary>
    /// Create Fail Result with data type
    /// </summary>
    /// <returns></returns>
    public static Result<T> Fail<T>(string message) => Result<T>.Fail(message);
    
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