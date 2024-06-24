namespace Senlin.Mo.Domain;

public static class DomainExtensions
{
    public static long Id(this string value) => Convert.ToInt64(value);
    
    
}