using System.Security.Cryptography;
using FileSystemMonitor.Api.Models;
using FileSystemMonitor.Api.Repositories;
using FileSystemMonitor.Contracts.Models;

namespace FileSystemMonitor.Api.Services;

public sealed class FileAnalyzer : IFileAnalyzer
{
    private readonly ISnapshotRepository _snapshots;
    private readonly ILogger<FileAnalyzer> _logger;

    public FileAnalyzer(ISnapshotRepository snapshots, ILogger<FileAnalyzer> logger)
    {
        _snapshots = snapshots;
        _logger = logger;
    }

public async Task<FileAnalysisResponse> AnalyzeAsync(
    string rootPath,
    CancellationToken cancellationToken)
{
    if (string.IsNullOrWhiteSpace(rootPath))
    {
        throw new ArgumentException(
            "Path is required.",
            nameof(rootPath));
    }

    var fullPath = Path.GetFullPath(rootPath);

    if (!Directory.Exists(fullPath))
    {
        throw new DirectoryNotFoundException(
            $"Directory does not exist: {fullPath}");
    }

    var previous = await _snapshots.GetLatestAsync(
        fullPath,
        cancellationToken);

    var version = (previous?.Version ?? 0) + 1;

    Console.WriteLine(
        $"Previous snapshot: " +
        $"{previous?.Version.ToString() ?? "NULL"}");

    // ------------------------------------------------------------
    // Previous files
    // ------------------------------------------------------------

    var previousFilesByPath =
        previous?.Files.ToDictionary(
            x => x.RelativePath,
            StringComparer.OrdinalIgnoreCase)
        ?? new Dictionary<string, FileSnapshotItem>(
            StringComparer.OrdinalIgnoreCase);

    // ------------------------------------------------------------
    // Previous directories
    // ------------------------------------------------------------

    var previousDirectories =
        previous?.Directories
            ?.Select(x => x.RelativePath)
            .ToHashSet(StringComparer.OrdinalIgnoreCase)
        ?? new HashSet<string>(
            StringComparer.OrdinalIgnoreCase);

    // ------------------------------------------------------------
    // Current analysis
    // ------------------------------------------------------------

    var currentFiles = new List<FileSnapshotItem>();

    var currentDirectories =
        new List<DirectorySnapshotItem>();

    var fileResults =
        new List<FileAnalysisItem>();

    var directoryResults =
        new List<DirectoryAnalysisItem>();

    // ============================================================
    // DIRECTORIES
    // ============================================================

    foreach (var directory in Directory.EnumerateDirectories(
                 fullPath,
                 "*",
                 SearchOption.AllDirectories))
    {
        cancellationToken.ThrowIfCancellationRequested();

        var relative =
            Path.GetRelativePath(
                fullPath,
                directory);

        currentDirectories.Add(
            new DirectorySnapshotItem
            {
                RelativePath = relative
            });

        var existedPreviously =
            previousDirectories.Remove(relative);

        FileChangeStatus status;

        if (previous is null)
        {
            // First analysis = baseline
            status = FileChangeStatus.Unchanged;
        }
        else if (existedPreviously)
        {
            status = FileChangeStatus.Unchanged;
        }
        else
        {
            status = FileChangeStatus.Added;
        }

        directoryResults.Add(
            new DirectoryAnalysisItem
            {
                RelativePath = relative,
                Status = status
            });
    }

    // Anything left in previousDirectories
    // no longer exists.
    foreach (var removedDirectory in previousDirectories)
    {
        directoryResults.Add(
            new DirectoryAnalysisItem
            {
                RelativePath = removedDirectory,
                Status = FileChangeStatus.Removed
            });
    }

    // ============================================================
    // FILES
    // ============================================================

    foreach (var file in Directory.EnumerateFiles(
                 fullPath,
                 "*",
                 SearchOption.AllDirectories))
    {
        cancellationToken.ThrowIfCancellationRequested();

        var info = new FileInfo(file);

        var relative =
            Path.GetRelativePath(
                fullPath,
                file);

        previousFilesByPath.TryGetValue(
            relative,
            out var old);

        string hash;

        // --------------------------------------------------------
        // Optimization:
        //
        // If size + LastWriteTime are unchanged, we assume
        // the content has not changed and reuse the old hash.
        // --------------------------------------------------------

        if (old is not null &&
            old.Size == info.Length &&
            old.LastModifiedUtc == info.LastWriteTimeUtc)
        {
            hash = old.Hash;
        }
        else
        {
            hash = await ComputeHashAsync(
                file,
                cancellationToken);
        }

        // --------------------------------------------------------
        // Determine file status
        // --------------------------------------------------------

        FileChangeStatus status;

        if (previous is null)
        {
            // First analysis = baseline
            status = FileChangeStatus.Unchanged;
        }
        else if (old is null)
        {
            status = FileChangeStatus.Added;
        }
        else if (string.Equals(
                     old.Hash,
                     hash,
                     StringComparison.Ordinal))
        {
            status = FileChangeStatus.Unchanged;
        }
        else
        {
            status = FileChangeStatus.Modified;
        }

        // --------------------------------------------------------
        // File version
        //
        // New file       -> v1
        // Unchanged      -> same version
        // Modified       -> previous version + 1
        // --------------------------------------------------------

        int fileVersion;

        if (old is null)
        {
            fileVersion = 1;
        }
        else if (status == FileChangeStatus.Modified)
        {
            fileVersion = old.Version + 1;
        }
        else
        {
            fileVersion = old.Version;
        }

        // --------------------------------------------------------
        // Save current file into snapshot
        // --------------------------------------------------------

        currentFiles.Add(
            new FileSnapshotItem
            {
                RelativePath = relative,
                Size = info.Length,
                LastModifiedUtc = info.LastWriteTimeUtc,
                Hash = hash,
                Version = fileVersion
            });

        // --------------------------------------------------------
        // Add file to analysis result
        // --------------------------------------------------------

        fileResults.Add(
            new FileAnalysisItem
            {
                RelativePath = relative,
                Size = info.Length,
                LastModifiedUtc = info.LastWriteTimeUtc,
                Hash = hash,
                Version = fileVersion,
                Status = status
            });

        // Remove from dictionary.
        // Whatever remains afterwards has been deleted.
        previousFilesByPath.Remove(relative);
    }

    // ============================================================
    // REMOVED FILES
    // ============================================================

    foreach (var removed in previousFilesByPath.Values)
    {
        fileResults.Add(
            new FileAnalysisItem
            {
                RelativePath = removed.RelativePath,
                Size = removed.Size,
                LastModifiedUtc = removed.LastModifiedUtc,
                Hash = removed.Hash,
                Version = removed.Version,
                Status = FileChangeStatus.Removed
            });
    }

    // ============================================================
    // RESPONSE
    // ============================================================

    var analyzedAtUtc = DateTime.UtcNow;

    var response =
        new FileAnalysisResponse
        {
            Version = version,

            PreviousVersion =
                previous?.Version,

            AnalyzedAtUtc =
                analyzedAtUtc,

            RootPath =
                fullPath,

            TotalFiles =
                currentFiles.Count,

            TotalDirectories =
                currentDirectories.Count,

            Changes =
                new FileChangeSummary
                {
                    Added =
                        fileResults.Count(
                            x => x.Status ==
                                 FileChangeStatus.Added),

                    Modified =
                        fileResults.Count(
                            x => x.Status ==
                                 FileChangeStatus.Modified),

                    Removed =
                        fileResults.Count(
                            x => x.Status ==
                                 FileChangeStatus.Removed),

                    Unchanged =
                        fileResults.Count(
                            x => x.Status ==
                                 FileChangeStatus.Unchanged),

                    AddedDirectories =
                        directoryResults.Count(
                            x => x.Status ==
                                 FileChangeStatus.Added),

                    RemovedDirectories =
                        directoryResults.Count(
                            x => x.Status ==
                                 FileChangeStatus.Removed),

                    UnchangedDirectories =
                        directoryResults.Count(
                            x => x.Status ==
                                 FileChangeStatus.Unchanged)
                },

            Files =
                fileResults
                    .OrderBy(
                        x => x.RelativePath,
                        StringComparer.OrdinalIgnoreCase)
                    .ToList(),

            Directories =
                directoryResults
                    .OrderBy(
                        x => x.RelativePath,
                        StringComparer.OrdinalIgnoreCase)
                    .ToList()
        };

    // ============================================================
    // SAVE SNAPSHOT
    // ============================================================

    await _snapshots.SaveAsync(
        fullPath,
        new FileSnapshot
        {
            Version = version,

            CreatedAtUtc =
                analyzedAtUtc,

            RootPath =
                fullPath,

            Files =
                currentFiles,

            Directories =
                currentDirectories
        },
        cancellationToken);

    _logger.LogInformation(
        "Analyzed {Path}: version {Version}, " +
        "{Files} files, {Directories} directories",
        fullPath,
        version,
        currentFiles.Count,
        currentDirectories.Count);

    return response;
}
    private static async Task<string> ComputeHashAsync(string path, CancellationToken cancellationToken)
    {
        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 64 * 1024, true);
        using var sha = SHA256.Create();
        var hash = await sha.ComputeHashAsync(stream, cancellationToken);
        return Convert.ToHexString(hash);
    }
}
