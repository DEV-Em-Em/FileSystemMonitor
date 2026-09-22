namespace FileSystemMonitor.Contracts.Models;

public sealed class FileAnalysisResponse
{
    public int Version { get; init; }

    public int? PreviousVersion { get; init; }

    public DateTime AnalyzedAtUtc { get; init; }

    public string RootPath { get; init; } = string.Empty;

    public int TotalFiles { get; init; }

    public int TotalDirectories { get; init; }

    public FileChangeSummary Changes { get; init; } = new();

    public IReadOnlyList<FileAnalysisItem> Files { get; init; } = [];

    public IReadOnlyList<DirectoryAnalysisItem> Directories { get; init; } = [];
}