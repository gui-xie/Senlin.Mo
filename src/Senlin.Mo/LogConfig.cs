namespace Senlin.Mo;

/// <summary>
/// Log Configuration
/// </summary>
/// <param name="Path">文件路径</param>
/// <param name="CountLimit">文件保留数量（天）</param>
/// <param name="Level">最小等级</param>
public record LogConfig(string Path, int CountLimit, string Level);