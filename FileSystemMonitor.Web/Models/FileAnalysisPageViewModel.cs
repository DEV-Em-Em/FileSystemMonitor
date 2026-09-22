using FileSystemMonitor.Contracts.Models;

namespace FileSystemMonitor.Web.Models;

public sealed class FileAnalysisPageViewModel
{
    public string Path { get; set; } = string.Empty;
    public FileAnalysisResponse? Analysis { get; set; }
    public string? Error { get; set; }
    public TreeNodeViewModel? Tree { get; set; }
}
