using FileSystemMonitor.Api.Services;
using FileSystemMonitor.Contracts.Models;
using Microsoft.AspNetCore.Mvc;

namespace FileSystemMonitor.Api.Controllers;

[ApiController]
[Route("api/filesystem")]
public sealed class FileSystemController : ControllerBase
{
    private readonly IFileAnalyzer _analyzer;

    public FileSystemController(IFileAnalyzer analyzer)
    {
        _analyzer = analyzer;
    }

    [HttpGet("analyze")]
    public async Task<ActionResult<FileAnalysisResponse>> Analyze(
        [FromQuery] string path,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _analyzer.AnalyzeAsync(path, cancellationToken));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (DirectoryNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = ex.Message });
        }
    }
}
