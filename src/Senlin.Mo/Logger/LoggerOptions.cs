namespace Senlin.Mo;

public class LoggerOptions
{
    public int CountLimit { get; set; } = 14;

    public string Path { get; set; } = "logs/log";

    public string Level { get; set; } = "Debug";

    public bool DisableDefaultMiddleware { get; set; }
}