using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ViveToolGUI.Views;

namespace ViveToolGUI;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        AdminInfo.IsOpen = !App.Elevation.IsAdministrator;
        NavView.SelectedItem = NavView.MenuItems[0];
        ContentFrame.Navigate(typeof(EnableDisablePage));
    }

    public void ShowInfo(string title, string message)
    {
        MessageBar.Severity = InfoBarSeverity.Informational;
        MessageBar.Title = title;
        MessageBar.Message = message;
        MessageBar.IsOpen = true;
    }

    public void ShowError(string title, string message)
    {
        MessageBar.Severity = InfoBarSeverity.Error;
        MessageBar.Title = title;
        MessageBar.Message = message;
        MessageBar.IsOpen = true;
    }

    private async void RestartAdmin_Click(object sender, RoutedEventArgs e)
    {
        await App.Elevation.RelaunchElevatedAsync();
    }

    private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is not NavigationViewItem item || item.Tag is not string tag)
        {
            return;
        }

        var page = tag switch
        {
            "EnableDisable" => typeof(EnableDisablePage),
            "Query" => typeof(QueryPage),
            "Advanced" => typeof(AdvancedCommandsPage),
            "Backup" => typeof(BackupRestorePage),
            "Reset" => typeof(ResetRepairPage),
            "Subfeatures" => typeof(SubfeaturesPage),
            "Settings" => typeof(SettingsPage),
            _ => typeof(EnableDisablePage)
        };

        ContentFrame.Navigate(page);
    }
}
