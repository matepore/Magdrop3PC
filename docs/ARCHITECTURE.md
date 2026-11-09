# Architecture Overview

Magdrop3PC is a Windows x64 project that builds a native executable for **Magical Drop III (Neo Geo MVS)** using the [neogeorecomp](https://github.com/matepore/neogeorecomp) static-recompilation toolkit.

## Repository components

### Magdrop3PC

The main repository contains the game-specific integration:

- **magdrop3/src/game.c** describes the game program and ROM layout expected by the toolkit.
- **magdrop3/src/main.c** starts the game through the toolkit runtime entry point, ng_main().
- **magdrop3/src/recomp_main.c** invokes ng_recomp_main() to generate native C sources.
- **magdrop3/CMakeLists.txt** defines the game generator and player targets, selects the regional system BIOS at build time, and prepares the standalone runtime directories.
- **MagicalDropLauncher.exe** is the Windows launcher in the repository root. Its source is maintained separately and is not included in this repository.

### neogeorecomp

The neogeorecomp directory is a Git submodule. It provides the Neo Geo machine model and runtime, including the 68000 execution/recompilation infrastructure, video, input, audio, and SDL2 window integration.

Keep toolkit-specific changes in the submodule repository rather than copying toolkit implementation files into the game repository. The submodule has its own license and third-party notices; the main repository's MIT license does not replace them.

### Launcher

The launcher provides the user-facing workflow for selecting a region, checking game files and build dependencies, building the selected version, and starting the game. Its executable is present in the repository, but its source is maintained locally rather than here.

The launcher is a build and startup helper. Users must still provide their own compatible game ROM and Neo Geo BIOS files.

## Build-time flow

A regional build has two relevant stages:

1. **Recompilation/generation:** the magdrop3_recomp target uses the game definition and supplied ROM/BIOS data to generate native C source files in the build directory.
2. **Game executable:** the magdrop3 target compiles the game entry point, game definition, generated sources (when configured), and neogeorecomp runtime into the executable.

Generated C files and other build products belong in the local build directory. They are not source files to commit.

The CMake project can also build an interpreter-only executable when no ROM directory is configured. A normal playable regional build uses the recompilation configuration supplied by the launcher or equivalent CMake cache settings.

## Runtime flow

At startup, magdrop3/src/main.c passes the game definition and, when compiled into the executable, the generated recompiled function table to ng_main().

The toolkit runtime supplies the emulated Neo Geo hardware services needed by the game code. When the SDL2 window backend is available, it also provides the desktop window, rendering, audio output, keyboard handling, and controller support.

The SDL2 backend stores user settings in settings.cfg beside the executable. The runtime resolves default game-data directories relative to the executable, so launching from a shortcut or a different working directory does not change where it looks for data.

## Runtime directory layout

Each regional executable should have its own local data/configuration area:

    Build-<Region>/
    ├── magdrop3.exe
    ├── SDL2.dll                 # when required by the Windows SDL2 build
    ├── settings.cfg             # created/updated by the game
    ├── rom/                     # user-supplied, uncompressed game ROM files
    └── bios/                    # user-supplied, uncompressed Neo Geo system files

The exact build directory name depends on the selected region and build configuration. CMake creates the empty rom/ and bios/ directories beside the executable after building. Users must populate them with compatible files.

## Separation of responsibilities

| Component | Responsibility |
|---|---|
| Main repository | Game-specific definition, CMake integration, launcher executable, project documentation |
| neogeorecomp submodule | Neo Geo runtime, recompiler, hardware model, SDL2 integration |
| Launcher | Region selection, dependency/game-data checks, build and launch workflow |
| Local ROM/BIOS directories | User-provided copyrighted game and system data |
| Build directory | Generated C sources, executable, build cache and other generated artifacts |

## Data and licensing boundary

Magdrop3PC source code does not grant rights to Magical Drop III or Neo Geo BIOS data. ROMs, BIOS files, generated code derived from game data, and other copyrighted game assets must remain outside public source commits unless their distribution is independently authorized.

The main repository's LICENSE applies only to original code covered by that license. The toolkit submodule and its dependencies retain their own licenses. See the root README for attribution and licensing notes.
