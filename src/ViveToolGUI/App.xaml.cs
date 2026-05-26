using Microsoft.UI.Xaml;
using ViveToolGUI.Services;

namespace ViveToolGUI;

public partial class App : Application
{
    public static AppSettingsService Settings { get; } = new();
    public static ElevationService Elevation { get; } = new();
    public static ViveToolRuntimeService Runtime { get; } = new(Settings);
    public static ViveToolService ViveTool { get; } = new(Runtime, Elevation);
    public static ViveToolUpdateService Updates { get; } = new(Runtime, ViveTool);

    public static MainWindow? MainWindow { get; private set; }

    public App()
    {
        InitializeComponent();
        UnhandledException += (_, e) =>
        {
            e.Handled = true;
            MainWindow?.ShowError("Unexpected error", e.Message);
        };
    }

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        await Runtime.EnsureSeededAsync();

        if (Settings.AlwaysRunAsAdministrator && !Elevation.IsAdministrator)
        {
            await Elevation.RelaunchElevatedAsync();
            return;
        }

        MainWindow = new MainWindow();
        MainWindow.Activate();
    }
}
