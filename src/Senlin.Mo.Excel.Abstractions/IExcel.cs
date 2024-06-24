namespace Senlin.Mo.Excel.Abstractions;

public interface IExcel
{
    string Name { get; }
    
    IReadOnlyCollection<string> UniqueColumn { get; }
    
    IReadOnlyCollection<IExcelColumn> Columns { get; }
}