# Magdrop3PC

A Windows static recompilation of **Magical Drop III** (Neo Geo MVS), built with the [neogeorecomp](https://github.com/matepore/neogeorecomp) toolkit.

The project recompiles the game's original code into a native executable while keeping game-specific code, the recompilation toolkit, and game data separate. It provides a foundation for running, studying, and modifying the game on Windows.

Game ROMs and Neo Geo BIOS files are not included. You must provide your own compatible files.

## Project Structure

- `magdrop3/` — Game-specific source code, CMake configuration, tests, and local game data directories.
- `magdrop3/roms/` — Location for the Magical Drop III ROM files.
- `magdrop3/bios/` — Location for the required Neo Geo BIOS files.
- `magdrop3/controls.cfg` — Optional local controller configuration.
- `magdrop3/Execute_Magdrop3_SDL2.bat` — Windows launcher for the SDL2 executable.
- `MagicalDropGUI.ps1` — PowerShell graphical interface for launching the game and configuring controls.
- `MagicalDropGUI.vbs` — Windows launcher for the graphical interface.
- `neogeorecomp/` — Git submodule containing the Neo Geo recompilation toolkit.
- `vcpkg/` — Local dependency-manager checkout used for the build; it is not included in the repository.

The `metalslug-reference/` directory used during development is maintained separately and is not required to build the project.

## Clone the Repository

Clone the repository and initialize its submodules:

```powershell
git clone --recurse-submodules https://github.com/matepore/Magdrop3PC.git
cd Magdrop3PC
```

If you already cloned the repository without its submodules, run:

```powershell
git submodule update --init --recursive
```

## Requirements

To build the SDL2 executable on Windows, you need:

- Windows x64
- Git
- CMake
- Visual Studio C++ Build Tools, including MSVC and the Windows SDK
- vcpkg
- SDL2 installed through vcpkg for the `x64-windows` triplet
- A compatible Magical Drop III MVS ROM set and the required Neo Geo BIOS files to run the game

## Set Up vcpkg and SDL2

The project uses [vcpkg](https://github.com/microsoft/vcpkg) to manage SDL2. If you do not already have vcpkg, open a terminal at the repository root and clone and bootstrap it:

```powershell
git clone https://github.com/microsoft/vcpkg.git
.\vcpkg\bootstrap-vcpkg.bat
```

Install SDL2 for the Windows x64 triplet:

```powershell
.\vcpkg\vcpkg.exe install sdl2:x64-windows
```

These commands place the vcpkg checkout in the repository root, which matches the toolchain path used in the build instructions below. If you keep vcpkg elsewhere, adjust the toolchain path accordingly.

## Build the SDL2 Executable

Open **Developer PowerShell for Visual Studio** with the x64 MSVC toolchain available. Run these commands from the repository root:

```powershell
cmake -S magdrop3 -B magdrop3/build-sdl2 `
  -G "NMake Makefiles" `
  -DCMAKE_BUILD_TYPE=Release `
  -DCMAKE_TOOLCHAIN_FILE="$PWD/vcpkg/scripts/buildsystems/vcpkg.cmake" `
  -DVCPKG_TARGET_TRIPLET=x64-windows

cmake --build magdrop3/build-sdl2 --target magdrop3
```

The expected executable location is:

```text
magdrop3/build-sdl2/magdrop3.exe
```

CMake must be able to locate SDL2 through the selected vcpkg toolchain. If configuration fails, verify that MSVC and NMake are available in the current terminal, SDL2 is installed for `x64-windows`, and the toolchain path points to the intended vcpkg checkout.

## Run the Game

First, place your ROM and BIOS files in the directories described in the **Game Data** section. From the repository root, run:

```powershell
.\magdrop3\build-sdl2\magdrop3.exe `
  --rom-path ".\magdrop3\roms" `
  --bios-path ".\magdrop3\bios"
```

Alternatively, launch the game using:

```text
magdrop3/Execute_Magdrop3_SDL2.bat
```

The launcher uses the executable at `magdrop3/build-sdl2/magdrop3.exe`.

## Game Data

This repository does not distribute Magical Drop III ROMs or Neo Geo BIOS files.

Place your own legally obtained, compatible files in these directories:

```text
magdrop3/roms/
magdrop3/bios/
```

The ROM set must correspond to the **MVS arcade version of Magical Drop III**. The required BIOS files must also be available for the selected game configuration.

Keep ROMs and BIOS files separate from source code and generated build outputs. Never commit game ROMs or BIOS files to the repository.

## Controller Configuration

Controller bindings are stored locally in:

```text
magdrop3/controls.cfg
```

If this file is missing, the game uses the default controls.

The PowerShell GUI (`MagicalDropGUI.ps1`) provides a graphical interface for launching the game and configuring controls. `MagicalDropGUI.vbs` can be used to start the GUI from Windows.

Controller binding capture requires the SDL2-enabled executable. The local `controls.cfg` file is excluded from version control so personal settings do not become part of the shared project.

## Dependency and Attribution

- **[neogeorecomp](https://github.com/matepore/neogeorecomp)** — Neo Geo recompilation toolkit used as the foundation of this project.
- **[Original neogeorecomp project](https://github.com/sp00nznet/neogeorecomp)** — Upstream project from which the toolkit originated.
- **[Metal Slug reference project](https://github.com/sp00nznet/metalslug)** — Reference implementation used during development.

The `neogeorecomp/` directory is a Git submodule with its own repository history. Toolkit changes are maintained separately from the main `Magdrop3PC` repository.

CMake, Visual Studio Build Tools, SDL2, and vcpkg are external dependencies and must be installed or obtained separately. Consult their respective projects for installation instructions and licensing terms.

Preserve the original copyright notices and licenses for the toolkit and all third-party dependencies when modifying or redistributing the project.
