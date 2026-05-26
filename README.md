# ViVeTool GUI

WinUI 3 rebuild of ViVeTool GUI.

## Requirements

- .NET 9 SDK for Visual Studio 2022
- .NET 10 SDK for Visual Studio 2026 Insider
- Windows App SDK 2.0.1
- winapp CLI

## Visual Studio configurations

Use the configuration dropdown or `Build > Configuration Manager`:

- VS 2022: `Debug-Net9 | x64` or `Release-Net9 | x64`
- VS 2026 Insider: `Debug-Net10 | x64` or `Release-Net10 | x64`

The project defaults to `.NET 9` when no custom configuration is selected so VS 2022 can load the solution.

## CLI build

```powershell
winget install Microsoft.DotNet.SDK.9
winget install Microsoft.DotNet.SDK.10
winget install Microsoft.winappcli
dotnet restore
dotnet build .\ViveToolGUI.sln -c Debug-Net9
dotnet build .\ViveToolGUI.sln -c Debug-Net10
dotnet publish .\src\ViveToolGUI\ViveToolGUI.csproj -c Release-Net10 -r win-x64
```

This repository includes ViVeTool binaries as package seed files only. At runtime the app copies them to `%LocalAppData%\ViveToolGUI\Tools\<arch>\current` or uses a user-selected custom folder.
