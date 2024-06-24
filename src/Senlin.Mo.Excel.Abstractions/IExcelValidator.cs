namespace Senlin.Mo.Excel.Abstractions;

public interface IExcelValidator<in T>
{
    IExcelValidationResult Validate(T value);
}