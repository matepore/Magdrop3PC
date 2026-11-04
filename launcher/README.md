# Magical Drop III Launcher

Native Windows launcher built with C# and WPF.

## Requirements

- Windows 10/11 x64
- .NET 10 SDK to build the launcher
- Visual Studio 2022/2026 or Build Tools with the MSVC C++ x64 toolchain
- CMake (available on PATH, installed separately, or included with Visual Studio)
- The repository's vcpkg directory and installed x64-windows dependencies

## Publish standalone launcher

From the repository root:

```powershell
dotnet publish .\launcher\MagicalDropLauncher\MagicalDropLauncher.csproj -c Release
```

The standalone executable is generated at:

```
launcher/MagicalDropLauncher/bin/Release/net10.0-windows/win-x64/publish/MagicalDropLauncher.exe
```

It includes the .NET runtime and does not require a console or a separately installed .NET Desktop Runtime to launch.

The launcher expects the game executable at `magdrop3/build-final/magdrop3.exe` by default. Use the Browse control to select another build output if needed.

ROMs and BIOS files are expected in `magdrop3/roms` and `magdrop3/bios`. The launcher never downloads or distributes game data.

Launcher preferences are stored locally under `%LOCALAPPDATA%\Magdrop3PC\Launcher\`.

## Building the game

The launcher uses the CMake `NMake Makefiles` generator and creates one independent build directory per BIOS region:

- `magdrop3/Build-Europe-Asia`
- `magdrop3/Build-USA`
- `magdrop3/Build-Japan`

You can start the launcher normally by double-clicking its executable. When BUILD is pressed, it locates Visual Studio's C++ x64 toolchain through Visual Studio Installer, initializes the MSVC environment in a hidden child process, and runs CMake without opening a console window. You do not need to start the launcher from a Developer Command Prompt.

The launcher itself can be opened normally by double-clicking its executable. Building the game still requires the MSVC C++ x64 toolchain, CMake, and the repository's installed vcpkg dependencies. The launcher discovers and initializes the compiler environment automatically in a hidden child process; no Developer Command Prompt or visible console is required.

The current runtime supports the MVS arcade configuration. AES / Console mode is not implemented and is therefore not offered by this launcher.
