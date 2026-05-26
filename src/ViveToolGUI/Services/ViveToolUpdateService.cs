using System.IO.Compression;
using System.Net.Http.Headers;
using System.Text.Json;
using ViveToolGUI.Models;

namespace ViveToolGUI.Services;

public sealed class ViveToolUpdateService(ViveToolRuntimeService runtime, ViveToolService viveTool)
{
    private static readonly HttpClient Http = CreateHttpClient();

    public async Task<ViveToolReleaseInfo?> CheckLatestReleaseAsync(CancellationToken cancellationToken = default)
    {
        using var response = await Http.GetAsync("https://api.github.com/repos/thebookisclosed/ViVe/releases/latest", cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        var root = document.RootElement;
        var tag = root.GetProperty("tag_name").GetString() ?? string.Empty;
        var htmlUrl = root.GetProperty("html_url").GetString() ?? string.Empty;
        var publishedAt = root.TryGetProperty("published_at", out var published) && DateTimeOffset.TryParse(published.GetString(), out var date)
            ? date
            : (DateTimeOffset?)null;

        foreach (var asset in root.GetProperty("assets").EnumerateArray())
        {
            var name = asset.GetProperty("name").GetString() ?? string.Empty;
            if (!IsAssetForCurrentArchitecture(name))
            {
                continue;
            }

            var url = asset.GetProperty("browser_download_url").GetString() ?? string.Empty;
            return new(tag, htmlUrl, name, url, publishedAt);
        }

        return null;
    }

    public async Task<CommandResult> ApplyReleaseAsync(ViveToolReleaseInfo release, CancellationToken cancellationToken = default)
    {
        if (!runtime.CanManageActiveFolder())
        {
            return new(release.AssetUrl, string.Empty, -1, string.Empty, "The selected custom folder is not app-managed.");
        }

        if (Directory.Exists(runtime.StagingDirectory))
        {
            Directory.Delete(runtime.StagingDirectory, recursive: true);
        }
        Directory.CreateDirectory(runtime.StagingDirectory);

        var zipPath = Path.Combine(runtime.StagingDirectory, release.AssetName);
        await using (var remote = await Http.GetStreamAsync(release.AssetUrl, cancellationToken))
        await using (var local = File.Create(zipPath))
        {
            await remote.CopyToAsync(local, cancellationToken);
        }

        ZipFile.ExtractToDirectory(zipPath, runtime.StagingDirectory, overwriteFiles: true);
        var candidate = Directory.EnumerateFiles(runtime.StagingDirectory, "ViVeTool.exe", SearchOption.AllDirectories)
            .Select(Path.GetDirectoryName)
            .FirstOrDefault(path => path is not null);

        if (candidate is null)
        {
            return new(release.AssetUrl, string.Empty, -1, string.Empty, "Downloaded release does not contain ViVeTool.exe.");
        }

        var validation = runtime.ValidateDirectory(candidate);
        if (!validation.IsValid)
        {
            return new(release.AssetUrl, string.Empty, -1, string.Empty, validation.Message);
        }

        var smoke = await viveTool.RunRawAsync(validation.ToolPath, "/?", cancellationToken);
        if (!smoke.Succeeded)
        {
            return smoke;
        }

        BackupCurrentRuntime();
        Directory.CreateDirectory(runtime.ActiveToolDirectory);
        foreach (var file in Directory.EnumerateFiles(candidate))
        {
            File.Copy(file, Path.Combine(runtime.ActiveToolDirectory, Path.GetFileName(file)), overwrite: true);
        }

        return new(validation.ToolPath, "apply-update", 0, $"Updated ViVeTool to {release.Version}.", string.Empty);
    }

    public Task<CommandResult> UpdateDictionaryAsync(CancellationToken cancellationToken = default)
    {
        if (!runtime.CanManageActiveFolder())
        {
            return Task.FromResult(new CommandResult(runtime.ActiveToolPath, "/dictupdate", -1, string.Empty, "The selected custom folder is not app-managed."));
        }

        var request = new ViveToolCommandRequest { Command = "dictupdate", RequiresAdministrator = false };
        return viveTool.RunAsync(request, cancellationToken);
    }

    private void BackupCurrentRuntime()
    {
        if (Directory.Exists(runtime.PreviousToolDirectory))
        {
            Directory.Delete(runtime.PreviousToolDirectory, recursive: true);
        }

        Directory.CreateDirectory(runtime.PreviousToolDirectory);
        if (!Directory.Exists(runtime.ActiveToolDirectory))
        {
            return;
        }

        foreach (var file in Directory.EnumerateFiles(runtime.ActiveToolDirectory))
        {
            File.Copy(file, Path.Combine(runtime.PreviousToolDirectory, Path.GetFileName(file)), overwrite: true);
        }
    }

    private bool IsAssetForCurrentArchitecture(string assetName)
    {
        var name = assetName.ToLowerInvariant();
        if (runtime.ArchitectureFolder == "arm64")
        {
            return name.Contains("arm64") || name.Contains("snapdragon");
        }

        return name.Contains("intel") || name.Contains("amd") || name.Contains("x64") || name.Contains("x86");
    }

    private static HttpClient CreateHttpClient()
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("ViveToolGUI", "1.0"));
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        return client;
    }
}
