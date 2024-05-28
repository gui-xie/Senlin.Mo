namespace Senlin.Mo;

public class ApplicationLoggerOptions
{
    public int CountLimit { get; set; } = 14;
    
    public string Path { get; set; } = "logs";
    
    public string Level { get; set; } = "Debug";
}