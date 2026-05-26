using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ViveToolGUI.Models;

namespace ViveToolGUI.Views;

public sealed partial class SubfeaturesPage : Page
{
    public SubfeaturesPage()
    {
        InitializeComponent();
    }

    private async void Query_Click(object sender, RoutedEventArgs e)
    {
        await RunAsync(new ViveToolCommandRequest { Command = "querysubs", RequiresAdministrator = false });
    }

    private async void Add_Click(object sender, RoutedEventArgs e)
    {
        var request = BuildFeatureRequest("addsub", true, includeOptions: true);
        if (request is not null)
        {
            await RunAsync(request);
        }
    }

    private async void Delete_Click(object sender, RoutedEventArgs e)
    {
        var request = BuildFeatureRequest("delsub", true, includeOptions: false);
        if (request is not null)
        {
            await RunAsync(request);
        }
    }

    private async void Notify_Click(object sender, RoutedEventArgs e)
    {
        var request = BuildFeatureRequest("notifyusage", true, includeTarget: false, includeOptions: true);
        if (request is not null)
        {
            await RunAsync(request);
        }
    }

    private ViveToolCommandRequest? BuildFeatureRequest(string command, bool requiresAdmin, bool includeTarget = true, bool includeOptions = true)
    {
        if (string.IsNullOrWhiteSpace(FeatureBox.Text))
        {
            Show("Input required", "Enter at least one feature ID or name.", InfoBarSeverity.Warning);
            return null;
        }

        var request = new ViveToolCommandRequest { Command = command, RequiresAdministrator = requiresAdmin };
        if (FeatureBox.Text.All(ch => char.IsDigit(ch) || ch == ',' || char.IsWhiteSpace(ch)))
        {
            request.Arguments.Add($"/id:{FeatureBox.Text.Replace(" ", string.Empty)}");
        }
        else
        {
            request.Arguments.Add($"/name:{FeatureBox.Text.Replace(" ", string.Empty)}");
        }
        request.Arguments.Add($"/reportingkind:{(int)KindBox.Value}");
        if (includeTarget)
        {
            request.Arguments.Add($"/reportingtarget:{TargetBox.Text.Trim()}");
        }
        if (includeOptions)
        {
            request.Arguments.Add($"/reportingoptions:{(int)OptionsBox.Value}");
        }
        return request;
    }

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
