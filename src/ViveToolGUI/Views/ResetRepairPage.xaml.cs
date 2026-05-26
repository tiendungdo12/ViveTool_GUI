using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ViveToolGUI.Models;

namespace ViveToolGUI.Views;

public sealed partial class ResetRepairPage : Page
{
    public ResetRepairPage()
    {
        InitializeComponent();
    }

    private async void Reset_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(IdsBox.Text) && string.IsNullOrWhiteSpace(NamesBox.Text))
        {
            Show("Input required", "Enter at least one ID or name.", InfoBarSeverity.Warning);
            return;
        }

        var request = new ViveToolCommandRequest { Command = "reset", RequiresAdministrator = true };
        if (!string.IsNullOrWhiteSpace(IdsBox.Text))
        {
            request.Arguments.Add($"/id:{IdsBox.Text.Replace(" ", string.Empty)}");
        }
        if (!string.IsNullOrWhiteSpace(NamesBox.Text))
        {
            request.Arguments.Add($"/name:{NamesBox.Text.Replace(" ", string.Empty)}");
        }
        request.Arguments.Add($"/store:{ComboValue(ResetStoreBox)}");
        await RunAsync(request);
    }

    private async void FullReset_Click(object sender, RoutedEventArgs e)
    {
        if (!await ConfirmAsync("Full reset", "This removes all custom feature configuration overrides. Continue?"))
        {
            return;
        }

        await RunAsync(new ViveToolCommandRequest { Command = "fullreset", RequiresAdministrator = true });
    }

    private async void FixLkg_Click(object sender, RoutedEventArgs e)
    {
        if (!await ConfirmAsync("Fix LKG", "This attempts to repair Last Known Good rollback data. Continue?"))
        {
            return;
        }

        await RunAsync(new ViveToolCommandRequest { Command = "fixlkg", RequiresAdministrator = true });
    }

    private async void FixPriority_Click(object sender, RoutedEventArgs e)
    {
        await RunAsync(new ViveToolCommandRequest { Command = "fixpriority", RequiresAdministrator = true });
    }

    private async void LkgStatus_Click(object sender, RoutedEventArgs e)
    {
        await RunAsync(new ViveToolCommandRequest { Command = "lkgstatus", RequiresAdministrator = false });
    }

    private async void ChangeStamp_Click(object sender, RoutedEventArgs e)
    {
        await RunAsync(new ViveToolCommandRequest { Command = "changestamp", RequiresAdministrator = false });
    }

    private async Task RunAsync(ViveToolCommandRequest request)
    {
        var result = await App.ViveTool.RunAsync(request);
        Show(result.Succeeded ? "Command completed" : "Command failed", result.CombinedOutput, result.Succeeded ? InfoBarSeverity.Success : InfoBarSeverity.Error);
    }

    private static string ComboValue(ComboBox box) => ((ComboBoxItem)box.SelectedItem).Content.ToString() ?? string.Empty;

    private async Task<bool> ConfirmAsync(string title, string content)
    {
        var dialog = new ContentDialog
        {
            XamlRoot = XamlRoot,
            Title = title,
            Content = content,
            PrimaryButtonText = "Continue",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close
        };

        return await dialog.ShowAsync() == ContentDialogResult.Primary;
    }

    private void Show(string title, string message, InfoBarSeverity severity)
    {
        ResultBar.Title = title;
        ResultBar.Message = message;
        ResultBar.Severity = severity;
        ResultBar.IsOpen = true;
    }
}
