using System.Text.RegularExpressions;
using ViveToolGUI.Models;

namespace ViveToolGUI.Services;

public static partial class FeatureQueryParser
{
    public static IReadOnlyList<FeatureConfigItem> Parse(string output)
    {
        var items = new List<FeatureConfigItem>();

        foreach (var line in output.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var match = QueryLineRegex().Match(line);
            if (!match.Success)
            {
                continue;
            }

            items.Add(new FeatureConfigItem
            {
                Id = match.Groups["id"].Value,
                Priority = match.Groups["priority"].Value,
                State = match.Groups["state"].Value,
                Type = match.Groups["type"].Value,
                Name = match.Groups["name"].Value.Trim()
            });
        }

        return items;
    }

    [GeneratedRegex(@"^(?<id>\d+)\s+(?<priority>\S+)\s+(?<state>Enabled|Disabled|Default|Unknown)\s+(?<type>\S+)(?:\s+(?<name>.*))?$", RegexOptions.IgnoreCase)]
    private static partial Regex QueryLineRegex();
}
