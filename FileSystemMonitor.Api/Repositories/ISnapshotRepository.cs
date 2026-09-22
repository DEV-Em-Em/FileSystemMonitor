using FileSystemMonitor.Api.Models;

namespace FileSystemMonitor.Api.Repositories;

public interface ISnapshotRepository
{
    Task<FileSnapshot?> GetLatestAsync(string rootPath, CancellationToken cancellationToken);
    Task SaveAsync(string rootPath, FileSnapshot snapshot, CancellationToken cancellationToken);
}
