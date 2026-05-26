using System.Runtime.InteropServices;
using ViveToolGUI.Models;

namespace ViveToolGUI.Services;

public sealed class ViveToolRuntimeService(AppSettingsService settings)
{
    private static readonly string[] RequiredFiles =
    [
        "ViVeTool.exe",
        "Albacore.ViVe.dll",
        "FeatureDictionary.pfs"
    ];

    public string ArchitectureFolder => RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ? "arm64" : "x64";

    public string LocalToolDirectory => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "ViveToolGUI",
        "Tools",
        ArchitectureFolder,
        "current");

    public string PreviousToolDirectory => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "ViveToolGUI",
        "Tools",
        ArchitectureFolder,
        "previous");

    public string StagingDirectory => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "ViveToolGUI",
        "Tools",
        ArchitectureFolder,
        "staging");

    public string BundledSeedDirectory => Path.Combine(AppContext.BaseDirectory, "Assets", "Tools", ArchitectureFolder);

    public string ActiveToolDirectory => settings.LocationMode == ToolLocationMode.Custom && !string.IsNullOrWhiteSpace(settings.CustomToolDirectory)
        ? settings.CustomToolDirectory
        : LocalToolDirectory;

    public string ActiveToolPath => Path.Combine(ActiveToolDirectory, "ViVeTool.exe");

    public async Task EnsureSeededAsync()
    {
        if (settings.LocationMode == ToolLocationMode.Custom)
        {
            return;
        }

        if (ValidateDirectory(LocalToolDirectory).IsValid)
        {
            return;
        }

        await RepairLocalToolAsync();
    }

    public Task RepairLocalToolAsync()
    {
        Directory.CreateDirectory(LocalToolDirectory);

        foreach (var file in Directory.EnumerateFiles(BundledSeedDirectory))
        {
            var destination = Path.Combine(LocalToolDirectory, Path.GetFileName(file));
            File.Copy(file, destination, overwrite: true);
        }

        return Task.CompletedTask;
    }

    public RuntimeValidationResult ValidateActiveRuntime()
    {
        return ValidateDirectory(ActiveToolDirectory);
    }

    public RuntimeValidationResult ValidateDirectory(string directory)
    {
        if (string.IsNullOrWhiteSpace(directory))
        {
            return new(false, directory, string.Empty, "No ViVeTool folder selected.");
        }

        if (!Directory.Exists(directory))
        {
            return new(false, directory, Path.Combine(directory, "ViVeTool.exe"), "ViVeTool folder does not exist.");
        }

        foreach (var file in RequiredFiles)
        {
            if (!File.Exists(Path.Combine(directory, file)))
            {
                return new(false, directory, Path.Combine(directory, "ViVeTool.exe"), $"Missing required file: {file}");
            }
        }

        return new(true, directory, Path.Combine(directory, "ViVeTool.exe"), "ViVeTool runtime is ready.");
    }

    public bool CanManageActiveFolder()
    {
        return settings.LocationMode == ToolLocationMode.LocalAppData || settings.AllowManagedUpdatesInCustomFolder;
    }
}
