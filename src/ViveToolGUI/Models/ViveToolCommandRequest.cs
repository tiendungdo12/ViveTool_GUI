namespace ViveToolGUI.Models;

public sealed class ViveToolCommandRequest
{
    public string Command { get; init; } = string.Empty;
    public List<string> Arguments { get; } = [];
    public bool RequiresAdministrator { get; init; }

    public string ToPreview()
    {
        return $"vivetool /{Command} {string.Join(' ', Arguments)}".Trim();
    }

    public string ToProcessArguments()
    {
        return $"/{Command} {string.Join(' ', Arguments.Select(QuoteIfNeeded))}".Trim();
    }

    private static string QuoteIfNeeded(string value)
    {
        if (!value.Contains(' ') && !value.Contains('"'))
        {
            return value;
        }

        return $"\"{value.Replace("\"", "\\\"")}\"";
    }
}
