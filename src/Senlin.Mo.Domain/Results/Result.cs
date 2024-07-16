namespace Senlin.Mo.Domain;

/// <summary>
/// Result
/// </summary>
public abstract class Result
{
    /// <summary>
    /// Create Success Result
    /// </summary>
    /// <returns></returns>
    public static IResult Success() => new R();

    /// <summary>
    /// Create Success Result with data type
    /// </summary>
    public static Task<IResult> SuccessTask() => Task.FromResult(Success());

    /// <summary>
    /// Create Error Result
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public static IResult Fail(string message) => new R(message);

    /// <summary>
    /// Create Success Result with data type
    /// </summary>
    /// <param name="data"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static IResult<T> Success<T>(T data) => R<T>.CreateSuccess(data);
    
    /// <summary>
    /// Create Error Result with data type
    /// </summary>
    /// <param name="message"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static IResult<T> Fail<T>(string message) => R<T>.CreateFailure(message);

    private class R<T> : IResult<T>
    {
        private R()
        {
        }

        public static R<T> CreateSuccess(T data)
        {
            return new R<T>
            {
                Data = data,
                Status = ResultStatus.Success,
                Message = string.Empty
            };
        }

        public static R<T> CreateFailure(string message)
        {
            return new R<T>
            {
                Data = default,
                Status = ResultStatus.Fail,
                Message = message
            };
        }

        public ResultStatus Status { get; private set; }
        public string Message { get; private set; } = string.Empty;
        public T? Data { get; private set; }
    }

    private class R : IResult
    {
        public R(string message)
        {
            Status = ResultStatus.Fail;
            Message = message;
        }

        public R()
        {
            Status = ResultStatus.Success;
            Message = string.Empty;
        }

        public ResultStatus Status { get; }
        public string Message { get; }
    }
}