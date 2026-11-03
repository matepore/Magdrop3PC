# Magical Drop III Launcher

Native Windows launcher built with C# and WPF.

## Requirements

- Windows 10/11 x64
- .NET 10 SDK
- Visual Studio 2026 with the .NET desktop development workload, or the .NET 10 SDK

## Build

From the repository root:

```powershell
dotnet build .\launcher\MagicalDropLauncher\MagicalDropLauncher.csproj -c Release
```

The launcher expects the game executable at `magdrop3/build-final/magdrop3.exe` by default. Use the Browse control to select another build output if needed.

ROMs and BIOS files are expected in `magdrop3/roms` and `magdrop3/bios`. The launcher never downloads or distributes game data.

Launcher preferences are stored locally under `%LOCALAPPDATA%\Magdrop3PC\Launcher\`.

The current runtime supports the MVS arcade configuration. AES / Console mode is not implemented and is therefore not offered by this launcher.
