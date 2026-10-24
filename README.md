# Magdrop3PC

A Windows-focused recompilation project for the MVS arcade version of *Magical Drop III*, built on the Neo Geo recompilation toolkit.

The goal is to keep game-specific code separate from the Neo Geo runtime while enabling modding and enhancements.

## Project structure

- `magdrop3/` — Game-specific source, tests, and CMake configuration.
- `MagicalDropGUI.ps1` — Windows GUI for launching the game and configuring controls.
- `MagicalDropGUI.vbs` — Windows launcher for the GUI.
- `neogeorecomp/` — Local checkout of the Neo Geo recompilation toolkit.
- `metalslug-reference/` — Local reference project.
- `vcpkg/` — Local C/C++ dependency manager checkout.

The three sibling repositories are maintained independently and are excluded from the root repository. The toolkit is currently expected beside `magdrop3/`.

## Requirements

The current Windows setup uses:

- Windows
- CMake
- Visual Studio C++ build tools
- SDL2
- vcpkg with the `x64-windows` triplet

The verified local configuration used the CMake generator `Visual Studio 18 2026`, platform `x64`, and the vcpkg toolchain in the local `vcpkg/` checkout.

The currently installed CMake reports `4.5.0-rc1`, which is a release candidate rather than a stable release. Exact tool versions may need adjustment on other systems.

## Build the SDL2 executable

Run these commands from the project root in PowerShell:

```powershell
cmake -S magdrop3 -B magdrop3/build-sdl2-game-vs-x64 `
  -G "Visual Studio 18 2026" `
  -A x64 `
  -DCMAKE_TOOLCHAIN_FILE="$PWD/vcpkg/scripts/buildsystems/vcpkg.cmake"

cmake --build magdrop3/build-sdl2-game-vs-x64 `
  --target magdrop3 --config Release
```

The expected executable location is:

```text
magdrop3/build-sdl2-game-vs-x64/Release/magdrop3.exe
```

The build directory is generated locally and is not tracked by Git.

## Game data

ROM and BIOS files are not included in this repository. Place legally obtained files in the local directories:

```text
magdrop3/roms/
magdrop3/bios/
```

The GUI supplies the ROM and BIOS directory paths when launching the executable. Build-time recompilation settings are separate CMake options; a blank `MAGDROP3_ROM_DIR` or `MAGDROP3_BIOS_DIR` in the CMake cache means those paths were not configured for that build-time step.

## Controller configuration

The GUI supports controller binding capture and saving controller assignments locally. The local `magdrop3/controls.cfg` file is excluded from version control to avoid committing personal settings.

Controller capture requires the SDL2-enabled executable.

## Dependency and attribution

`neogeorecomp` is a separate project by its original author, `sp00nznet`. Its upstream repository is:

https://github.com/sp00nznet/neogeorecomp

The local checkout contains project-specific SDL2 and controller-capture changes. Those changes are committed in the nested repository, not in the Magdrop3PC root history.

Before publishing this project, the dependency should be connected through a stable, accessible remote—likely a fork maintained for Magdrop3PC—and referenced at a specific commit, for example using a Git submodule. Preserve upstream license and copyright notices, as well as notices for bundled dependencies.

`metalslug-reference/` and `vcpkg/` are local-only checkouts at present.

## Version-control exclusions

The root `.gitignore` excludes:

- ROM and BIOS directories
- Build output directories and compiled binaries
- Generated screenshots and verification logs
- Local controller settings
- The sibling repositories managed independently

Review staged files before every commit, and never commit ROMs, BIOS dumps, or generated build artifacts.
