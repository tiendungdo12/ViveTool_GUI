namespace ViveToolGUI.Models;

public sealed record ViveToolReleaseInfo(
    string Version,
    string ReleaseUrl,
    string AssetName,
    string AssetUrl,
    DateTimeOffset? PublishedAt);
