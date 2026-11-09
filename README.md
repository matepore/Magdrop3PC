# Magdrop3PC

A Windows x64 static recompilation of **Magical Drop III** (Neo Geo MVS), built with the [neogeorecomp](https://github.com/matepore/neogeorecomp) toolkit.

The project recompiles the game's original program into a native executable while keeping game-specific integration, the recompilation toolkit, and game data separate. It provides a foundation for running, studying, and modifying the game on Windows.

## Features

The in-game overlay is opened with **F1** by default.

- **Keyboard and controller configuration** — remap keyboard keys and controller inputs, assign controllers to players, and save the configuration.
- **Window scaling** — select window sizes from 2x to 5x.
- **Aspect ratios** — Original, 4:3, and 16:9 presentation options.
- **Fullscreen and VSync** — configurable display behavior.
- **Display filters** — Nearest Neighbor, Bilinear, and a lightweight CRT-style scanline effect.
- **Audio controls** — master volume and mute.
- **Overlay utilities** — pause/resume and screenshot capture.

The CRT option currently draws alternating dark horizontal lines; it is not a full CRT shader.

## Repository layout

- `MagicalDropLauncher.exe` — Windows x64 launcher for region selection, game-data/dependency checks, building, and starting the game. Its source is maintained separately and is not included here.
- `magdrop3/` — game-specific source code and CMake project.
- `neogeorecomp/` — Git submodule containing the Neo Geo recompilation toolkit.
- `rom/` and `bios/` — local game and system data directories used by the launcher/build workflow; do not commit copyrighted data.
- `vcpkg/` — optional local dependency-manager checkout for development; it is not included in the repository.
- `docs/` — project architecture, build, runtime configuration, graphics, input, and troubleshooting documentation.

## Getting started

1. Clone the repository and initialize its submodules:

   ```powershell
   git clone --recurse-submodules https://github.com/matepore/Magdrop3PC.git
   cd Magdrop3PC
   ```

   If the repository was cloned without submodules, run:

   ```powershell
   git submodule update --init --recursive
   ```

2. Obtain a compatible **Magical Drop III MVS ROM set** and the required Neo Geo system BIOS files. You must provide these files yourself.
3. Use the launcher to select a region, check prerequisites and build that region, or follow the manual build guide.
4. Place the uncompressed game ROM files in `rom/` and the required uncompressed system files in `bios/`, beside the resulting `magdrop3.exe`.
5. Run `magdrop3.exe` directly. No command-line arguments are required when using the default folder names.

The runtime resolves its default data directories relative to the executable, not the process working directory. ZIP archives are not read directly. Command-line overrides `--rom-path`, `--bios-path`, and `--bios` remain available.

## Documentation

| Document | Contents |
|---|---|
| [Architecture](docs/ARCHITECTURE.md) | Repository components, build/runtime flow, and separation of responsibilities |
| [Building](docs/BUILDING.md) | Windows development dependencies, vcpkg/SDL2 setup, and regional CMake builds |
| [Configuration](docs/CONFIGURATION.md) | ROM/BIOS layout, command-line overrides, and `settings.cfg` |
| [Graphics](docs/GRAPHICS.md) | Window size, aspect ratio, fullscreen, VSync, filters, and scaling behavior |
| [Input](docs/INPUT.md) | Default keyboard controls, controller support, remapping, and device identity |
| [Troubleshooting](docs/TROUBLESHOOTING.md) | Common data, SDL2, graphics, settings, and controller issues |

## Development notes

- The project targets Windows x64 for its launcher and normal game executable workflow.
- Regional builds should use separate build directories and the corresponding regional BIOS configuration.
- SDL2 is required for the windowed desktop runtime. Without it, the toolkit can build a headless-only runtime.
- Build outputs, generated C sources, local configuration, ROMs, and BIOS files should remain out of source commits.
- The project uses a manual build workflow; it does not depend on an automated release pipeline.

## License, dependencies and third-party content

Unless otherwise noted, original code authored for Magdrop3PC is distributed under the [MIT License](LICENSE). This license does not apply to the `neogeorecomp` submodule or its third-party dependencies, which retain their own licenses.

The project license does not grant rights to *Magical Drop III* or other third-party game content, including ROMs, BIOS files, generated game code, artwork, audio, screenshots, or trademarks. Those remain subject to their respective rights and licenses. Users are responsible for supplying data they are legally entitled to use.

Preserve the original copyright notices and licenses for the toolkit and all third-party dependencies when modifying or redistributing the project.
