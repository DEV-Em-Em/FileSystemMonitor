using System.Text.Json;
using FileSystemMonitor.Api.Models;

namespace FileSystemMonitor.Api.Repositories;

public sealed class JsonSnapshotRepository : ISnapshotRepository
{
    private readonly string _baseDirectory;
    private readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    public JsonSnapshotRepository(IWebHostEnvironment environment)
    {
        _baseDirectory = Path.Combine(environment.ContentRootPath, "Data", "Snapshots");
        Directory.CreateDirectory(_baseDirectory);
    }

    public async Task<FileSnapshot?> GetLatestAsync(string rootPath, CancellationToken cancellationToken)
    {
        var directory = GetDirectory(rootPath);
        if (!Directory.Exists(directory))
            return null;

        var files = Directory.EnumerateFiles(directory, "*.json")
            .Select(Path.GetFileNameWithoutExtension)
            .Select(x => int.TryParse(x, out var v) ? v : 0)
            .Where(x => x > 0)
            .OrderByDescending(x => x)
            .ToList();

        if (files.Count == 0)
            return null;

        var path = Path.Combine(directory, $"{files[0]}.json");
        await using var stream = File.OpenRead(path);
        return await JsonSerializer.DeserializeAsync<FileSnapshot>(stream, _options, cancellationToken);
    }

    public async Task SaveAsync(string rootPath, FileSnapshot snapshot, CancellationToken cancellationToken)
    {
        var directory = GetDirectory(rootPath);
        Directory.CreateDirectory(directory);

        var path = Path.Combine(directory, $"{snapshot.Version}.json");
        await using var stream = File.Create(path);
        await JsonSerializer.SerializeAsync(stream, snapshot, _options, cancellationToken);
    }

    private string GetDirectory(string rootPath)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(Path.GetFullPath(rootPath).TrimEnd(Path.DirectorySeparatorChar).ToUpperInvariant());
        var hash = Convert.ToHexString(sha.ComputeHash(bytes))[..16];
        return Path.Combine(_baseDirectory, hash);
    }
}
