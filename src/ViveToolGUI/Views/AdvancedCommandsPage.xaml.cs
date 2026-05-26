using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ViveToolGUI.Models;

namespace ViveToolGUI.Views;

public sealed partial class AdvancedCommandsPage : Page
{
    public AdvancedCommandsPage()
    {
        InitializeComponent();
    }

    private async void RunRaw_Click(object sender, RoutedEventArgs e)
    {
        var raw = RawArgsBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(raw))
        {
            Show("Input required", "Enter a ViVeTool argument string.", InfoBarSeverity.Warning);
            return;
        }

        var command = raw.TrimStart('/').Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? string.Empty;
        var request = new ViveToolCommandRequest { Command = command, RequiresAdministrator = RequiresAdminBox.IsChecked == true };
        foreach (var arg in raw.Split(' ', StringSplitOptions.RemoveEmptyEntries).Skip(1))
        {
            request.Arguments.Add(arg);
        }
        await RunAsync(request);
    }

    private async void LkgStatus_Click(object sender, RoutedEventArgs e) => await RunAsync(new ViveToolCommandRequest { Command = "lkgstatus", RequiresAdministrator = false });
    private async void ChangeStamp_Click(object sender, RoutedEventArgs e) => await RunAsync(new ViveToolCommandRequest { Command = "changestamp", RequiresAdministrator = false });
    private async void QuerySubs_Click(object sender, RoutedEventArgs e) => await RunAsync(new ViveToolCommandRequest { Command = "querysubs", RequiresAdministrator = false });

    private async Task RunAsync(ViveToolCommandRequest request)
    {
        var result = await App.ViveTool.RunAsync(request);
        OutputBox.Text = result.CombinedOutput;
        Show(result.Succeeded ? "Command completed" : "Command failed", result.Succeeded ? request.ToPreview() : result.CombinedOutput, result.Succeeded ? InfoBarSeverity.Success : InfoBarSeverity.Error);
    }

    private void Show(string title, string message, InfoBarSeverity severity)
    {
        ResultBar.Title = title;
        ResultBar.Message = message;
        ResultBar.Severity = severity;
        ResultBar.IsOpen = true;
    }
}
