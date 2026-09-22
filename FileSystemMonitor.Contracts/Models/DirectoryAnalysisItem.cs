namespace FileSystemMonitor.Contracts.Models;

public sealed class DirectoryAnalysisItem
{
    public string RelativePath { get; init; } = string.Empty;

    public FileChangeStatus Status { get; init; }
}