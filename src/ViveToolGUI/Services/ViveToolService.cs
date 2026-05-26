using System.Diagnostics;
using System.Text;
using ViveToolGUI.Models;

namespace ViveToolGUI.Services;

public sealed class ViveToolService(ViveToolRuntimeService runtime, ElevationService elevation)
{
    private readonly SemaphoreSlim _commandLock = new(1, 1);

    public bool IsBusy => _commandLock.CurrentCount == 0;

    public async Task<CommandResult> RunAsync(ViveToolCommandRequest request, CancellationToken cancellationToken = default)
    {
        var validation = runtime.ValidateActiveRuntime();
        if (!validation.IsValid)
        {
            return new(validation.ToolPath, request.ToProcessArguments(), -1, string.Empty, validation.Message);
        }

        if (request.RequiresAdministrator && !elevation.IsAdministrator)
        {
            var relaunched = await elevation.RelaunchElevatedAsync();
            return new(validation.ToolPath, request.ToProcessArguments(), -2, string.Empty, relaunched ? "Relaunching as administrator." : "Administrator approval was cancelled or failed.");
        }

        await _commandLock.WaitAsync(cancellationToken);
        try
        {
            return await RunRawAsync(validation.ToolPath, request.ToProcessArguments(), cancellationToken);
        }
        finally
        {
            _commandLock.Release();
        }
    }

    public async Task<CommandResult> RunRawAsync(string fileName, string arguments, CancellationToken cancellationToken = default)
    {
        var stdout = new StringBuilder();
        var stderr = new StringBuilder();

        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                WorkingDirectory = Path.GetDirectoryName(fileName) ?? AppContext.BaseDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            },
            EnableRaisingEvents = true
        };

        process.OutputDataReceived += (_, e) =>
        {
            if (e.Data is not null)
            {
                stdout.AppendLine(e.Data);
            }
        };
        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data is not null)
            {
                stderr.AppendLine(e.Data);
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        await process.WaitForExitAsync(cancellationToken);

        return new(fileName, arguments, process.ExitCode, stdout.ToString().Trim(), stderr.ToString().Trim());
    }

    public Task<CommandResult> QueryAsync(string extraArgs = "", CancellationToken cancellationToken = default)
    {
        var request = new ViveToolCommandRequest { Command = "query", RequiresAdministrator = false };
        if (!string.IsNullOrWhiteSpace(extraArgs))
        {
            request.Arguments.Add(extraArgs);
        }
        return RunAsync(request, cancellationToken);
    }
}
