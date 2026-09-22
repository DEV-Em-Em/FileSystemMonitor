using FileSystemMonitor.Contracts.Models;

namespace FileSystemMonitor.Web.Models;

public sealed class TreeNodeViewModel
{
    public string Name { get; init; } = string.Empty;

    public int Depth { get; init; }

    public FileChangeStatus Status { get; set; } =
        FileChangeStatus.Unchanged;

    public List<TreeNodeViewModel> Folders { get; } = [];

    public List<FileAnalysisItem> Files { get; } = [];

}