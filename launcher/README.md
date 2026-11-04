# Magical Drop III Launcher

Native Windows launcher built with C# and WPF.

## Requirements

- Windows 10/11 x64
- .NET 10 SDK
- .NET 10 SDK
- Visual Studio 2026 (or compatible Visual Studio installation) with the C++ x64 build tools and NMake

## Build

From the repository root:

```powershell
dotnet build .\launcher\MagicalDropLauncher\MagicalDropLauncher.csproj -c Release
```

The launcher expects the game executable at `magdrop3/build-final/magdrop3.exe` by default. Use the Browse control to select another build output if needed.

ROMs and BIOS files are expected in `magdrop3/roms` and `magdrop3/bios`. The launcher never downloads or distributes game data.

Launcher preferences are stored locally under `%LOCALAPPDATA%\Magdrop3PC\Launcher\`.

## Building the game

The launcher uses the CMake `NMake Makefiles` generator for all regional builds. It checks that `nmake.exe`, `cl.exe`, and the MSVC `INCLUDE`/`LIB` environment are available before configuring. If they are not, start the launcher from the x64 Native Tools Command Prompt for your Visual Studio installation.

Regional build trees are created separately as `magdrop3/Build-Europe-Asia-NMake`, `magdrop3/Build-USA-NMake`, and `magdrop3/Build-Japan-NMake`. Existing build directories created with another CMake generator are left untouched.

The current runtime supports the MVS arcade configuration. AES / Console mode is not implemented and is therefore not offered by this launcher.
