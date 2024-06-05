using Senlin.Mo.Localization.Abstractions;

namespace Senlin.Mo.Domain;

/// <summary>
/// Localization string Result
/// </summary>
/// <typeparam name="T"></typeparam>
public class LResult<T>
{
    private LResult()
    {
    }

    public static LResult<T> Success(T data)
    {
        return new LResult<T>()
        {
            Data = data,
            Status = ResultStatus.Success
        };
    }

    public static LResult<T> Fail(LString error)
    {
        return new LResult<T>
        {
            Error = error,
            Status = ResultStatus.Fail
        };
    }

    public ResultStatus Status { get; private set; }

    public T Data { get; private set; }

    public LString Error { get; private set; }

    public Result<T> ToResult(LStringResolver r) =>
        Status == ResultStatus.Success 
            ? Result<T>.Success(Data) 
            : Result<T>.Fail(r.Resolve(Error));
}