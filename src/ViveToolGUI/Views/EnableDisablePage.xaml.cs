using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ViveToolGUI.Models;

namespace ViveToolGUI.Views;

public sealed partial class EnableDisablePage : Page
{
    public EnableDisablePage()
    {
        InitializeComponent();
        UpdatePreview();
    }

    private async void Enable_Click(object sender, RoutedEventArgs e) => await ExecuteAsync("enable");

    private async void Disable_Click(object sender, RoutedEventArgs e) => await ExecuteAsync("disable");

    private async Task ExecuteAsync(string command)
    {
        var request = BuildRequest(command);
        if (request is null)
        {
            Show("Input required", "Enter at least one feature ID or feature name.", InfoBarSeverity.Warning);
            return;
        }

        var result = await App.ViveTool.RunAsync(request);
        Show(result.Succeeded ? "Command completed" : "Command failed", result.CombinedOutput, result.Succeeded ? InfoBarSeverity.Success : InfoBarSeverity.Error);
    }

    private ViveToolCommandRequest? BuildRequest(string command)
    {
        if (string.IsNullOrWhiteSpace(IdsBox.Text) && string.IsNullOrWhiteSpace(NamesBox.Text))
        {
            return null;
        }

        var request = new ViveToolCommandRequest { Command = command, RequiresAdministrator = true };
        if (!string.IsNullOrWhiteSpace(IdsBox.Text))
        {
            request.Arguments.Add($"/id:{IdsBox.Text.Replace(" ", string.Empty)}");
        }
        if (!string.IsNullOrWhiteSpace(NamesBox.Text))
        {
            request.Arguments.Add($"/name:{NamesBox.Text.Replace(" ", string.Empty)}");
        }
        request.Arguments.Add($"/variant:{(int)VariantBox.Value}");
        request.Arguments.Add($"/variantpayloadkind:{ComboValue(PayloadKindBox)}");
        request.Arguments.Add($"/variantpayload:{(uint)PayloadBox.Value}");
        if (ExperimentBox.IsChecked == true)
        {
            request.Arguments.Add("/experiment");
        }
        request.Arguments.Add($"/priority:{ComboValue(PriorityBox)}");
        request.Arguments.Add($"/store:{ComboValue(StoreBox)}");
        return request;
    }

    private void InputChanged(object sender, object e) => UpdatePreview();

    private void UpdatePreview()
    {
        PreviewBox.Text = BuildRequest("enable")?.ToPreview() ?? "Enter an ID or name to preview the command.";
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
