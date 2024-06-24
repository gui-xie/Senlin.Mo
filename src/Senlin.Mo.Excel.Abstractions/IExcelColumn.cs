namespace Senlin.Mo.Excel.Abstractions;

public interface IExcelColumn
{
    string Name { get; }

    string DisplayName { get; }

    int Width { get; }

    ExcelColumnType Type { get; }

    string Comment { get; }
}