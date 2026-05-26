namespace ViveToolGUI.Models;

public sealed record RuntimeValidationResult(
    bool IsValid,
    string ToolDirectory,
    string ToolPath,
    string Message);
