using Windows.Storage;

namespace ViveToolGUI.Services;

public enum ToolLocationMode
{
    LocalAppData,
    Custom
}

public enum UpdateInterval
{
    Daily,
    Weekly,
    Monthly
}

public sealed class AppSettingsService
{
    private readonly ApplicationDataContainer _settings = ApplicationData.Current.LocalSettings;

    public bool AlwaysRunAsAdministrator
    {
        get => Get("AlwaysRunAsAdministrator", false);
        set => Set("AlwaysRunAsAdministrator", value);
    }

    public ToolLocationMode LocationMode
    {
        get => Enum.TryParse<ToolLocationMode>(Get("LocationMode", ToolLocationMode.LocalAppData.ToString()), out var mode) ? mode : ToolLocationMode.LocalAppData;
        set => Set("LocationMode", value.ToString());
    }

    public string CustomToolDirectory
    {
        get => Get("CustomToolDirectory", string.Empty);
        set => Set("CustomToolDirectory", value);
    }

    public bool AllowManagedUpdatesInCustomFolder
    {
        get => Get("AllowManagedUpdatesInCustomFolder", false);
        set => Set("AllowManagedUpdatesInCustomFolder", value);
    }

    public bool AutoCheckViveToolUpdates
    {
        get => Get("AutoCheckViveToolUpdates", false);
        set => Set("AutoCheckViveToolUpdates", value);
    }

    public bool AutoCheckDictionaryUpdates
    {
        get => Get("AutoCheckDictionaryUpdates", false);
        set => Set("AutoCheckDictionaryUpdates", value);
    }

    public UpdateInterval UpdateInterval
    {
        get => Enum.TryParse<UpdateInterval>(Get("UpdateInterval", UpdateInterval.Weekly.ToString()), out var interval) ? interval : UpdateInterval.Weekly;
        set => Set("UpdateInterval", value.ToString());
    }

    public DateTimeOffset? LastUpdateCheck
    {
        get
        {
            var raw = Get("LastUpdateCheck", string.Empty);
            return DateTimeOffset.TryParse(raw, out var value) ? value : null;
        }
        set => Set("LastUpdateCheck", value?.ToString("O") ?? string.Empty);
    }

    private T Get<T>(string key, T fallback)
    {
        return _settings.Values.TryGetValue(key, out var value) && value is T typed ? typed : fallback;
    }

    private void Set(string key, object value)
    {
        _settings.Values[key] = value;
    }
}
