namespace Senlin.Mo.Excel.Abstractions;

public interface IExcelValidationResultError
{
    ExcelValidationError Error { get; }

    IReadOnlyCollection<int> ErrorRows { get; }
}