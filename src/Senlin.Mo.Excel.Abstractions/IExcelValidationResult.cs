namespace Senlin.Mo.Excel.Abstractions;

public interface IExcelValidationResult
{
    IReadOnlyCollection<IExcelValidationResultError> Errors { get; }

    bool HasError { get; }
}