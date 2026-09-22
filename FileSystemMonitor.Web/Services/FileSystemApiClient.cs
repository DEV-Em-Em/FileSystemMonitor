using System.Net.Http.Json;
using FileSystemMonitor.Contracts.Models;

namespace FileSystemMonitor.Web.Services;

public sealed class FileSystemApiClient : IFileSystemApiClient
{
    private readonly HttpClient _httpClient;

    public FileSystemApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<FileAnalysisResponse> AnalyzeAsync(string path, CancellationToken cancellationToken)
    {
        var url = $"api/filesystem/analyze?path={Uri.EscapeDataString(path)}";
        using var response = await _httpClient.GetAsync(url, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"API returned {(int)response.StatusCode} {response.ReasonPhrase}: {body}");
        }

        return await response.Content.ReadFromJsonAsync<FileAnalysisResponse>(cancellationToken: cancellationToken)
               ?? throw new InvalidOperationException("API returned an empty response.");
    }
}
