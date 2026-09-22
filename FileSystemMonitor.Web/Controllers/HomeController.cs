using FileSystemMonitor.Contracts.Models;
using FileSystemMonitor.Web.Models;
using FileSystemMonitor.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace FileSystemMonitor.Web.Controllers;

public sealed class HomeController : Controller
{
    private readonly IFileSystemApiClient _api;

    public HomeController(IFileSystemApiClient api)
    {
        _api = api;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View(new FileAnalysisPageViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Analyze(
        string path,
        CancellationToken cancellationToken)
    {
        var model = new FileAnalysisPageViewModel
        {
            Path = path
        };

        try
        {
            model.Analysis =
                await _api.AnalyzeAsync(
                    path,
                    cancellationToken);

            model.Tree =
                BuildTree(model.Analysis);
        }
        catch (Exception ex)
        {
            model.Error = ex.Message;
        }

        return View("Index", model);
    }

    private static TreeNodeViewModel BuildTree(
        Contracts.Models.FileAnalysisResponse analysis)
    {
        var rootName =
            Path.GetFileName(
                analysis.RootPath.TrimEnd(
                    Path.DirectorySeparatorChar,
                    Path.AltDirectorySeparatorChar));

        if (string.IsNullOrWhiteSpace(rootName))
        {
            rootName = analysis.RootPath;
        }

        var root = new TreeNodeViewModel
        {
            Name = rootName,
            Depth = 0
        };

        // ========================================================
        // DIRECTORIES
        // ========================================================

        foreach (var directory in analysis.Directories)
        {
            AddDirectory(
                root,
                directory);
        }

        // ========================================================
        // FILES
        // ========================================================

        foreach (var file in analysis.Files)
        {
            AddFile(
                root,
                file);
        }

        return root;
    }

   private static void AddDirectory(
    TreeNodeViewModel root,
    Contracts.Models.DirectoryAnalysisItem directory)
{
    var parts =
        directory.RelativePath.Split(
            new[] { '/', '\\' },
            StringSplitOptions.RemoveEmptyEntries);

    if (parts.Length == 0)
    {
        return;
    }

    var current = root;

    for (var i = 0; i < parts.Length; i++)
    {
        var part = parts[i];

        var folder =
            current.Folders.FirstOrDefault(
                x => x.Name.Equals(
                    part,
                    StringComparison.OrdinalIgnoreCase));

        if (folder == null)
        {
            folder = new TreeNodeViewModel
            {
                Name = part,
                Depth = current.Depth + 1,
                Status = FileChangeStatus.Unchanged
            };

            current.Folders.Add(folder);
        }

        // Posledná časť cesty je skutočný adresár
        // z API response, takže nastavíme jeho status.
        if (i == parts.Length - 1)
        {
            folder.Status = directory.Status;
        }

        current = folder;
    }
}

private static void AddFile(
    TreeNodeViewModel root,
    Contracts.Models.FileAnalysisItem file)
{
    var parts =
        file.RelativePath.Split(
            new[] { '/', '\\' },
            StringSplitOptions.RemoveEmptyEntries);

    if (parts.Length == 0)
    {
        return;
    }

    var current = root;

    for (var i = 0; i < parts.Length; i++)
    {
        var part = parts[i];

        // Posledná časť cesty je súbor.
        if (i == parts.Length - 1)
        {
            current.Files.Add(file);
            continue;
        }

        var folder =
            current.Folders.FirstOrDefault(
                x => x.Name.Equals(
                    part,
                    StringComparison.OrdinalIgnoreCase));

        if (folder == null)
        {
            folder = new TreeNodeViewModel
            {
                Name = part,
                Depth = current.Depth + 1,
                Status = FileChangeStatus.Unchanged
            };

            current.Folders.Add(folder);
        }

        current = folder;
    }
}
    
}