namespace FileSystemMonitor.Contracts.Models;

public sealed class FileAnalysisItem
{
    public string RelativePath { get; init; } = string.Empty;
    public long Size { get; init; }
    public DateTime LastModifiedUtc { get; init; }
    public string? Hash { get; init; }
    public int Version { get; init; }
    public FileChangeStatus Status { get; init; }
}
