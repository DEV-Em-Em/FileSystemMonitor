namespace FileSystemMonitor.Contracts.Models;

public sealed class FileChangeSummary
{
    public int Added { get; init; }

    public int Modified { get; init; }

    public int Removed { get; init; }

    public int Unchanged { get; init; }

    public int AddedDirectories { get; init; }

    public int RemovedDirectories { get; init; }

    public int UnchangedDirectories { get; init; }
}