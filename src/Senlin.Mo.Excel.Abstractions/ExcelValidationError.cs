namespace Senlin.Mo.Excel.Abstractions;

public enum ExcelValidationError
{
    Required,
    MinLength,
    MaxLength,
    MinValue,
    MaxValue,
    Unique,
    Enum,
    Type
}