namespace Senlin.Mo.Domain;

public class Result
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
    /// Create Error Result
    /// </summary>
    /// <param name="getMessage"></param>
    /// <returns></returns>
    public static IResult Fail(Func<string> getMessage) => new R(getMessage);
    
    /// <summary>
    /// Create Success Result with data type
    /// </summary>
    /// <param name="data"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static IResult<T> Success<T>(T data) => R<T>.CreateSuccess(data);
    
    /// <summary>
    /// Create Success Result with data type
    /// </summary>
    /// <param name="data"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static Task<IResult<T>> SuccessTask<T>(T data) => Task.FromResult(Success(data));
    
    /// <summary>
    /// Create Error Result with data type
    /// </summary>
    /// <param name="message"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static IResult<T> Fail<T>(string message) => R<T>.CreateFailure(message);

    /// <summary>
    /// Create Error Result with data type
    /// </summary>
    /// <param name="getMessage"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static IResult<T> Fail<T>(GetResultMessage getMessage) => R<T>.CreateFailure(getMessage);
    
    private class R<T> : IResult<T>
    {
        private T? _data;
        private string _message = string.Empty;
        private GetResultMessage? _getMessage;


        private R()
        {
        }

        public static R<T> CreateSuccess(T data)
        {
            return new R<T>
            {
                _data = data,
                IsSuccess = true,
                _message = string.Empty
            };
        }

        public static R<T> CreateFailure(string message)
        {
            return new R<T>
            {
                _data = default,
                IsSuccess = false,
                _message = message
            };
        }

        public static R<T> CreateFailure(GetResultMessage getMessage)
        {
            return new R<T>
            {
                _data = default,
                IsSuccess = false,
                _message = string.Empty,
                _getMessage = getMessage
            };
        }

        public T? GetData()
        {
            return _data;
        }

        public bool IsSuccess { get; private set; }
        public string GetErrorMessage() => _getMessage?.Invoke() ?? _message;
    }

    private class R : IResult
    {
        private readonly string _message;

        private readonly Func<string>? _getMessage;

        public R(string message)
        {
            IsSuccess = false;
            _message = message;
        }

        public R(Func<string> getMessage)
        {
            IsSuccess = false;
            _getMessage = getMessage;
            _message = string.Empty;
        }

        public R()
        {
            IsSuccess = true;
            _message = string.Empty;
        }

        public bool IsSuccess { get; }

        public string GetErrorMessage() => _getMessage?.Invoke() ?? _message;
    }
}