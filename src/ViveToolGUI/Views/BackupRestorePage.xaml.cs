using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ViveToolGUI.Models;

namespace ViveToolGUI.Views;

public sealed partial class BackupRestorePage : Page
{
    public BackupRestorePage()
    {
        InitializeComponent();
    }

    private async void Export_Click(object sender, RoutedEventArgs e)
    {
        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), $"vivetool-export-{DateTime.Now:yyyyMMdd-HHmmss}.bin");
        var request = new ViveToolCommandRequest { Command = "export", RequiresAdministrator = false };
        request.Arguments.Add($"/filename:{path}");
        request.Arguments.Add($"/store:{ComboValue(ExportStoreBox)}");
        var result = await App.ViveTool.RunAsync(request);
        Show(result.Succeeded ? "Export completed" : "Export failed", result.Succeeded ? path : result.CombinedOutput, result.Succeeded ? InfoBarSeverity.Success : InfoBarSeverity.Error);
    }

    private async void Import_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(ImportPathBox.Text) || !File.Exists(ImportPathBox.Text))
        {
            Show("Import file missing", "Enter a valid exported configuration path.", InfoBarSeverity.Warning);
            return;
        }

        var request = new ViveToolCommandRequest { Command = "import", RequiresAdministrator = true };
        request.Arguments.Add($"/filename:{ImportPathBox.Text}");
        request.Arguments.Add($"/store:{ComboValue(ImportStoreBox)}");
        if (ReplaceBox.IsChecked == true)
        {
            request.Arguments.Add("/replace");
        }

        var result = await App.ViveTool.RunAsync(request);
        Show(result.Succeeded ? "Import completed" : "Import failed", result.CombinedOutput, result.Succeeded ? InfoBarSeverity.Success : InfoBarSeverity.Error);
    }

    private static string ComboValue(ComboBox box) => ((ComboBoxItem)box.SelectedItem).Content.ToString() ?? string.Empty;

    private void Show(string title, string message, InfoBarSeverity severity)
    {
        ResultBar.Title = title;
        ResultBar.Message = message;
        ResultBar.Severity = severity;
        ResultBar.IsOpen = true;
    }
}
