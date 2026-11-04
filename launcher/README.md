# Magical Drop III Launcher

Native Windows launcher built with C# and WPF.

## Requirements

- Windows 10/11 x64
- .NET 10 SDK to build the launcher
- Visual Studio 2022/2026 or Build Tools with the MSVC C++ x64 toolchain
- CMake (available on PATH, installed separately, or included with Visual Studio)
- The repository's vcpkg directory and installed x64-windows dependencies

## Build

From the repository root:

```powershell
dotnet build .\launcher\MagicalDropLauncher\MagicalDropLauncher.csproj -c Release
```

The launcher expects the game executable at `magdrop3/build-final/magdrop3.exe` by default. Use the Browse control to select another build output if needed.

ROMs and BIOS files are expected in `magdrop3/roms` and `magdrop3/bios`. The launcher never downloads or distributes game data.

Launcher preferences are stored locally under `%LOCALAPPDATA%\Magdrop3PC\Launcher\`.

## Building the game

The launcher uses the CMake `NMake Makefiles` generator and creates one independent build directory per BIOS region:

- `magdrop3/Build-Europe-Asia`
- `magdrop3/Build-USA`
- `magdrop3/Build-Japan`

You can start the launcher normally by double-clicking its executable. When BUILD is pressed, it locates Visual Studio's C++ x64 toolchain through Visual Studio Installer, initializes the MSVC environment in a hidden child process, and runs CMake without opening a console window. You do not need to start the launcher from a Developer Command Prompt.

The machine still needs the C++ build tools, CMake, and the repository's vcpkg dependencies installed; those are the compiler/build dependencies, not launcher console requirements.

The current runtime supports the MVS arcade configuration. AES / Console mode is not implemented and is therefore not offered by this launcher.
