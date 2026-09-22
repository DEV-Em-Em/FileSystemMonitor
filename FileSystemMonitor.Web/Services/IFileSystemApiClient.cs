using FileSystemMonitor.Contracts.Models;

namespace FileSystemMonitor.Web.Services;

public interface IFileSystemApiClient
{
    Task<FileAnalysisResponse> AnalyzeAsync(string path, CancellationToken cancellationToken);
}
