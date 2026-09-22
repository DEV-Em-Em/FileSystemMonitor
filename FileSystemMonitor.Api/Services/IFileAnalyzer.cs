using FileSystemMonitor.Contracts.Models;

namespace FileSystemMonitor.Api.Services;

public interface IFileAnalyzer
{
    Task<FileAnalysisResponse> AnalyzeAsync(string rootPath, CancellationToken cancellationToken);
}
