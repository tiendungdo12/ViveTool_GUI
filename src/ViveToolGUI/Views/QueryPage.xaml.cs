using System.Collections.ObjectModel;
using System.Text;
using System.Text.Json;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ViveToolGUI.Models;
using ViveToolGUI.Services;

namespace ViveToolGUI.Views;

public sealed partial class QueryPage : Page
{
    private readonly List<FeatureConfigItem> _allFeatures = [];
    private CancellationTokenSource? _filterDebounce;

    public ObservableCollection<FeatureConfigItem> VisibleFeatures { get; } = [];

    public QueryPage()
    {
        InitializeComponent();
    }

    private async void QueryAll_Click(object sender, RoutedEventArgs e)
    {
        var result = await App.ViveTool.QueryAsync();
        if (!result.Succeeded)
        {
            Show("Query failed", result.CombinedOutput, InfoBarSeverity.Error);
            return;
        }

        _allFeatures.Clear();
        _allFeatures.AddRange(FeatureQueryParser.Parse(result.StandardOutput));
        ApplyFilters();
        Show("Query completed", $"Loaded {_allFeatures.Count} feature rows.", InfoBarSeverity.Success);
    }

    private async void FilterChanged(object sender, TextChangedEventArgs e)
    {
        _filterDebounce?.Cancel();
        _filterDebounce = new CancellationTokenSource();
        var token = _filterDebounce.Token;
        try
        {
            await Task.Delay(250, token);
            if (!token.IsCancellationRequested)
            {
                ApplyFilters();
            }
        }
        catch (TaskCanceledException)
        {
        }
    }

    private void ApplyFilters()
    {
        var id = IdFilterBox.Text.Trim();
        var state = StateFilterBox.Text.Trim();
        var priority = PriorityFilterBox.Text.Trim();
        var type = TypeFilterBox.Text.Trim();
        var name = NameFilterBox.Text.Trim();

        var filtered = _allFeatures.Where(item =>
            (id.Length < 3 || item.Id.Contains(id, StringComparison.OrdinalIgnoreCase)) &&
            (state.Length == 0 || item.State.Contains(state, StringComparison.OrdinalIgnoreCase)) &&
            (priority.Length == 0 || item.Priority.Contains(priority, StringComparison.OrdinalIgnoreCase)) &&
            (type.Length == 0 || item.Type.Contains(type, StringComparison.OrdinalIgnoreCase)) &&
            (name.Length == 0 || item.Name.Contains(name, StringComparison.OrdinalIgnoreCase)));

        VisibleFeatures.Clear();
        foreach (var item in filtered.Take(10000))
        {
            VisibleFeatures.Add(item);
        }
    }

    private async void ExportTxt_Click(object sender, RoutedEventArgs e) => await ExportAsync("txt");
    private async void ExportCsv_Click(object sender, RoutedEventArgs e) => await ExportAsync("csv");
    private async void ExportJson_Click(object sender, RoutedEventArgs e) => await ExportAsync("json");

    private async Task ExportAsync(string format)
    {
        var folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var path = Path.Combine(folder, $"vivetool-query-{DateTime.Now:yyyyMMdd-HHmmss}.{format}");
        var text = format switch
        {
            "csv" => ToCsv(),
            "json" => JsonSerializer.Serialize(VisibleFeatures, new JsonSerializerOptions { WriteIndented = true }),
            _ => ToText()
        };

        await File.WriteAllTextAsync(path, text, Encoding.UTF8);
        Show("Export completed", path, InfoBarSeverity.Success);
    }

    private string ToText() => string.Join(Environment.NewLine, VisibleFeatures.Select(x => $"{x.Id}\t{x.Priority}\t{x.State}\t{x.Type}\t{x.Name}"));

    private string ToCsv()
    {
        static string Esc(string value) => $"\"{value.Replace("\"", "\"\"")}\"";
        var rows = new List<string> { "ID,Priority,State,Type,Name" };
        rows.AddRange(VisibleFeatures.Select(x => string.Join(',', Esc(x.Id), Esc(x.Priority), Esc(x.State), Esc(x.Type), Esc(x.Name))));
        return string.Join(Environment.NewLine, rows);
    }

    private void Show(string title, string message, InfoBarSeverity severity)
    {
        ResultBar.Title = title;
        ResultBar.Message = message;
        ResultBar.Severity = severity;
        ResultBar.IsOpen = true;
    }
}
