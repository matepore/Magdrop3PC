# Magdrop3PC

## Project Status and Development Notes

This project was developed primarily with the assistance of artificial intelligence tools, particularly ChatGPT.

**Development status:** The project is currently on hold. I have no plans for further development at the moment and intend to take a break. There is no active roadmap or expected schedule for future updates.

A Windows x64 static recompilation of **Magical Drop III** (Neo Geo MVS), built with the [neogeorecomp](https://github.com/matepore/neogeorecomp) toolkit.

The project recompiles the game's original code into a native executable while keeping game-specific code, the recompilation toolkit, and game data separate. It provides a foundation for running, studying, and modifying the game on Windows.

## Implemented Enhancements

This project includes several enhancements to the original game, improving its input configuration, display options, and audio controls. To access them you will need to press the F1 key in-game.

- **In-Game Keyboard and Controller Binding Configuration** — Customize keyboard keys and controller buttons directly from the game.
- **Custom Window Scaling** — Choose window sizes ranging from 2x to 5x the original resolution.
- **Aspect Ratio Options** — Switch between the original aspect ratio, 4:3, and 16:9.
- **Fullscreen Mode** — Play in fullscreen.
- **VSync** — Enable or disable vertical synchronization.
- **Display Filters** — Choose between Nearest Neighbor, Bilinear, and CRT filters.
- **Audio Controls** — Adjust the game volume or mute the audio.

## Project Structure

- `MagicalDropLauncher.exe` — Windows x64 launcher for checking game files, selecting a game region, checking build dependencies, and starting the game.
- `magdrop3/` — Game-specific source code, CMake configuration, tests, and local game data directories.
- `rom/` — Uncompressed Magical Drop III ROM files, placed beside the game executable.
- `bios/` — Required uncompressed Neo Geo BIOS files, placed beside the game executable.
- `magdrop3/Build-*/settings.cfg` — Local game configuration, including controls, graphics, and sound settings.
- `magdrop3/Build-<Region>/entries.txt` — Recorded missing entries for the selected region, when miss recording is enabled.
- `neogeorecomp/` — Git submodule containing the Neo Geo recompilation toolkit.
- `vcpkg/` — Local dependency-manager checkout used for development builds; it is not included in the repository.

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

## Getting Started

1. Obtain a compatible **Magical Drop III MVS ROM set** and the required Neo Geo BIOS files.
2. Place the uncompressed game ROM files in `rom/` and the required Neo Geo system files in `bios/`, both beside the built game executable. See [Game Data](#game-data) for the file requirements.
3. Run `magdrop3.exe` directly. No command-line arguments are required when using these default folder names.

The launcher includes a dependency checker and can guide you through installing or configuring missing build dependencies. Some installation steps may require an internet connection, administrator permissions, or interaction with external installers.

## Game Data

This project requires **uncompressed BIOS and ROM files**. It does not support `.zip` or similar archives.

Place your compatible files in these directories, next to `magdrop3.exe`:

```text
Build-<Region>/
├── magdrop3.exe
├── SDL2.dll
├── rom/
└── bios/
```

CMake creates the empty `rom/` and `bios/` directories beside the executable after building. You must supply the files yourself. The runtime resolves these paths relative to the executable, not the process working directory. Existing command-line overrides (`--rom-path`, `--bios-path`, and `--bios`) remain available.

The ROM set must correspond to the **MVS arcade version of Magical Drop III**. The required BIOS files must also be available for the selected game configuration.

## Local Configuration and Generated Files

The game stores configuration separately for each build region:

```text
magdrop3/Build-<Region>/settings.cfg
```

This file contains local settings for controls, graphics, and sound. If it is missing, the game starts with default settings.

When **Record misses for next build** is enabled in the launcher, missing entries are recorded in:

```text
magdrop3/Build-<Region>/entries.txt
```

The runtime merges new entries with existing ones, removes duplicates, and sorts the resulting list when the game exits.

## Developer Information

The launcher is intended to simplify setup and dependency checks for players. Developers who want to build the SDL2 executable manually need the following tools on Windows x64:

- Git, including support for submodules
- CMake
- Visual Studio C++ Build Tools, including MSVC and the Windows SDK
- NMake, available through the Visual Studio developer environment
- vcpkg
- SDL2 installed through vcpkg for the `x64-windows` triplet

### Set Up vcpkg and SDL2

If vcpkg is not already available, clone and bootstrap it from the repository root:

```powershell
git clone https://github.com/microsoft/vcpkg.git
.\\vcpkg\\bootstrap-vcpkg.bat
```

Install SDL2 for the Windows x64 triplet:

```powershell
.\\vcpkg\\vcpkg.exe install sdl2:x64-windows
```

These commands place the vcpkg checkout in the repository root, matching the toolchain path used below. If vcpkg is stored elsewhere, adjust the toolchain path accordingly.

### Build the SDL2 Executable

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

Select the system BIOS for a regional build with `-DMAGDROP3_BIOS_FILE=<filename>` during CMake configuration. The default is `sp-s2.sp1` (MVS Asia/Europe).

## Known Limitations

- The launcher and game executable target Windows x64.
- Game ROM and BIOS files must be supplied separately and must be uncompressed.
- Building from source requires the external development dependencies listed above.
- The launcher source is maintained locally and is not included in this repository; the compiled launcher executable is distributed in the repository root.
- Installing missing dependencies may require an internet connection, administrator permissions, or manual interaction with external installers.
- Miss recording depends on the game's runtime reaching the relevant code paths; it should not be considered an exhaustive list of every possible missing entry.
- Game data and build outputs are region-specific.
- The runtime expects `rom/` and `bios/` beside the executable by default; both folders must contain compatible, uncompressed files.

## License, Dependency and Attribution

Unless otherwise noted, original code authored for **Magdrop3PC** is distributed under the [MIT License](LICENSE). This license does not apply to the **neogeorecomp** submodule or its third-party dependencies, which retain their own licenses. It also does not grant any rights to *Magical Drop III* or other third-party game content, including ROMs, BIOS files, artwork, audio, screenshots, or trademarks; those remain subject to their respective rights and licenses.

- **[neogeorecomp](https://github.com/matepore/neogeorecomp)** — Neo Geo recompilation toolkit used as the foundation of this project.
- **[Original neogeorecomp project](https://github.com/sp00nznet/neogeorecomp)** — Upstream project from which the toolkit originated.

CMake, Visual Studio Build Tools, SDL2, and vcpkg are external dependencies and must be installed or obtained separately. Consult their respective projects for installation instructions and licensing terms.

Preserve the original copyright notices and licenses for the toolkit and all third-party dependencies when modifying or redistributing the project.
