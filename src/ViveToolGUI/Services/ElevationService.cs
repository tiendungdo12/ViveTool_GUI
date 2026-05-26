using System.ComponentModel;
using System.Diagnostics;
using System.Security.Principal;

namespace ViveToolGUI.Services;

public sealed class ElevationService
{
    public bool IsAdministrator
    {
        get
        {
            using var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }

    public async Task<bool> RelaunchElevatedAsync()
    {
        try
        {
            var exe = Environment.ProcessPath ?? Process.GetCurrentProcess().MainModule?.FileName;
            if (string.IsNullOrWhiteSpace(exe))
            {
                return false;
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = exe,
                UseShellExecute = true,
                Verb = "runas"
            });

            await Task.Delay(150);
            Environment.Exit(0);
            return true;
        }
        catch (Win32Exception)
        {
            return false;
        }
    }
}
