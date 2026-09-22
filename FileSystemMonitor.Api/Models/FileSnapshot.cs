namespace FileSystemMonitor.Api.Models;

public sealed class FileSnapshot
{
    public int Version { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public string RootPath { get; init; } = string.Empty;

    public List<FileSnapshotItem> Files { get; init; } = [];

    public List<DirectorySnapshotItem> Directories { get; init; } = [];
}

public sealed class FileSnapshotItem
{
    public string RelativePath { get; init; } = string.Empty;
    public long Size { get; init; }
    public DateTime LastModifiedUtc { get; init; }
    public string Hash { get; init; } = string.Empty;

    // Verzia konkrétneho súboru
    public int Version { get; init; }
}

public sealed class DirectorySnapshotItem
{
    public string RelativePath { get; init; } = string.Empty;
}