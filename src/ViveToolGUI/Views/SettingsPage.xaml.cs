using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ViveToolGUI.Services;

namespace ViveToolGUI.Views;

public sealed partial class SettingsPage : Page
{
    private Models.ViveToolReleaseInfo? _latestRelease;

    public SettingsPage()
    {
        InitializeComponent();
        LoadSettings();
        RefreshStatus();
    }

    private void LoadSettings()
    {
        AlwaysAdminSwitch.IsOn = App.Settings.AlwaysRunAsAdministrator;
        LocationModeButtons.SelectedIndex = App.Settings.LocationMode == ToolLocationMode.LocalAppData ? 0 : 1;
        CustomFolderBox.Text = App.Settings.CustomToolDirectory;
        ManageCustomFolderBox.IsChecked = App.Settings.AllowManagedUpdatesInCustomFolder;
        AutoToolUpdateSwitch.IsOn = App.Settings.AutoCheckViveToolUpdates;
        AutoDictionaryUpdateSwitch.IsOn = App.Settings.AutoCheckDictionaryUpdates;
        UpdateIntervalBox.SelectedIndex = App.Settings.UpdateInterval switch
        {
            UpdateInterval.Daily => 0,
            UpdateInterval.Monthly => 2,
            _ => 1
        };
    }

    private void RefreshStatus()
    {
        AdminStatusText.Text = App.Elevation.IsAdministrator ? "Current process is elevated." : "Current process is not elevated.";
        var validation = App.Runtime.ValidateActiveRuntime();
        RuntimeStatusText.Text = $"{validation.Message}{Environment.NewLine}{validation.ToolDirectory}";
        UpdateStatusText.Text = App.Settings.LastUpdateCheck is { } last
            ? $"Last checked: {last.LocalDateTime}"
            : "Updates have not been checked yet.";
    }

    private void AlwaysAdminSwitch_Toggled(object sender, RoutedEventArgs e)
    {
        App.Settings.AlwaysRunAsAdministrator = AlwaysAdminSwitch.IsOn;
    }

    private async void RestartAdmin_Click(object sender, RoutedEventArgs e)
    {
        await App.Elevation.RelaunchElevatedAsync();
    }

    private void LocationModeButtons_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        App.Settings.LocationMode = LocationModeButtons.SelectedIndex == 1 ? ToolLocationMode.Custom : ToolLocationMode.LocalAppData;
        RefreshStatus();
    }

    private void CustomFolderBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        App.Settings.CustomToolDirectory = CustomFolderBox.Text.Trim();
        RefreshStatus();
    }

    private void ManageCustomFolderBox_Changed(object sender, RoutedEventArgs e)
    {
        App.Settings.AllowManagedUpdatesInCustomFolder = ManageCustomFolderBox.IsChecked == true;
    }

    private void AutoToolUpdateSwitch_Toggled(object sender, RoutedEventArgs e)
    {
        App.Settings.AutoCheckViveToolUpdates = AutoToolUpdateSwitch.IsOn;
    }

    private void AutoDictionaryUpdateSwitch_Toggled(object sender, RoutedEventArgs e)
    {
        App.Settings.AutoCheckDictionaryUpdates = AutoDictionaryUpdateSwitch.IsOn;
    }

    private void UpdateIntervalBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        App.Settings.UpdateInterval = UpdateIntervalBox.SelectedIndex switch
        {
            0 => UpdateInterval.Daily,
            2 => UpdateInterval.Monthly,
            _ => UpdateInterval.Weekly
        };
    }

    private void ValidateRuntime_Click(object sender, RoutedEventArgs e)
    {
        RefreshStatus();
        Show("Runtime validation", App.Runtime.ValidateActiveRuntime().Message, App.Runtime.ValidateActiveRuntime().IsValid ? InfoBarSeverity.Success : InfoBarSeverity.Error);
    }

    private async void RepairRuntime_Click(object sender, RoutedEventArgs e)
    {
        await App.Runtime.RepairLocalToolAsync();
        RefreshStatus();
        Show("Runtime repaired", App.Runtime.LocalToolDirectory, InfoBarSeverity.Success);
    }

    private async void CheckToolUpdate_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var release = await App.Updates.CheckLatestReleaseAsync();
            _latestRelease = release;
            App.Settings.LastUpdateCheck = DateTimeOffset.Now;
            UpdateStatusText.Text = release is null
                ? "No matching ViVeTool release asset was found for this architecture."
                : $"Latest ViVeTool release: {release.Version} ({release.AssetName})";
            Show("ViVeTool update check completed", UpdateStatusText.Text, release is null ? InfoBarSeverity.Warning : InfoBarSeverity.Informational);
        }
        catch (Exception ex)
        {
            Show("ViVeTool update check failed", ex.Message, InfoBarSeverity.Error);
        }
    }

    private async void ApplyToolUpdate_Click(object sender, RoutedEventArgs e)
    {
        if (_latestRelease is null)
        {
            Show("Check required", "Run Check for ViVeTool Update first.", InfoBarSeverity.Warning);
            return;
        }

        try
        {
            var result = await App.Updates.ApplyReleaseAsync(_latestRelease);
            RefreshStatus();
            Show(result.Succeeded ? "ViVeTool updated" : "ViVeTool update failed", result.CombinedOutput, result.Succeeded ? InfoBarSeverity.Success : InfoBarSeverity.Error);
        }
        catch (Exception ex)
        {
            Show("ViVeTool update failed", ex.Message, InfoBarSeverity.Error);
        }
    }

    private async void CheckDictionaryUpdate_Click(object sender, RoutedEventArgs e)
    {
        var result = await App.Updates.UpdateDictionaryAsync();
        App.Settings.LastUpdateCheck = DateTimeOffset.Now;
        RefreshStatus();
        Show(result.Succeeded ? "Dictionary update completed" : "Dictionary update failed", result.CombinedOutput, result.Succeeded ? InfoBarSeverity.Success : InfoBarSeverity.Error);
    }

    private void Show(string title, string message, InfoBarSeverity severity)
    {
        ResultBar.Title = title;
        ResultBar.Message = message;
        ResultBar.Severity = severity;
        ResultBar.IsOpen = true;
    }
}
