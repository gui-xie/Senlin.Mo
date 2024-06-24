namespace Senlin.Mo.Excel.Abstractions;

public interface IExcelHelper
{
    Task ImportAsync(Stream stream);
}