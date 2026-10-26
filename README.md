# Magdrop3PC

A Windows-focused static recompilation project for the MVS arcade version of *Magical Drop III*, built on the Neo Geo recompilation toolkit.

The goal is to keep game-specific code separate from the Neo Geo runtime and game data, making future modifications and enhancements easier to maintain.

## Project structure

- `magdrop3/` — Game-specific source code, tests, CMake configuration, and local game data directories.
- `MagicalDropGUI.ps1` — Windows GUI for launching the game and configuring controls.
- `MagicalDropGUI.vbs` — Windows launcher for the GUI.
- `neogeorecomp/` — Git submodule containing the Neo Geo recompilation toolkit, pinned to a specific commit.
- `metalslug-reference/` — Local reference project, maintained separately from the root repository.
- `vcpkg/` — Local vcpkg checkout used to manage C/C++ dependencies.

The root repository does not include the contents of the `metalslug-reference/` and `vcpkg/` directories. They must be obtained and configured separately.

The toolkit is tracked as a Git submodule. When cloning the repository, initialize it with:

```powershell
git clone --recurse-submodules https://github.com/matepore/Magdrop3PC.git
```

If the repository has already been cloned without its submodules, initialize them from the project root:

```powershell
git submodule update --init --recursive
```

A fresh setup must also provide the required local reference project and vcpkg checkout, with compatible versions and configuration.

## Requirements

The current Windows development setup uses:

- Windows
- CMake
- Visual Studio C++ build tools
- SDL2
- vcpkg with the `x64-windows` triplet

The verified local configuration used the CMake generator `Visual Studio 18 2026`, platform `x64`, and the toolchain file in the local `vcpkg/` checkout.

The installed CMake version in that environment was `4.5.0-rc1`, a release candidate rather than a stable release. Other environments may require different tool versions or generator settings.

The commands below assume that the Visual Studio generator is available, the required C++ components are installed, vcpkg is configured, and SDL2 can be resolved through the selected toolchain.

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

These commands create a separate build configuration. Test the resulting executable before treating it as equivalent to the existing, manually verified build.

Keep build directories separate when changing CMake generators, toolchains, or build options. Do not reuse a build directory configured for a different generator or toolchain.

## Run the game

The existing, manually verified SDL2 executable is located at:

```text
magdrop3/build-sdl2/magdrop3.exe
```

The Windows launcher is:

```text
magdrop3/Execute_Magdrop3_SDL2.bat
```

The launcher changes its working directory to the game folder and passes the local ROM and BIOS directory paths to the executable.

To run the existing executable directly, open PowerShell in `magdrop3/` and use:

```powershell
.\build-sdl2\magdrop3.exe `
  --rom-path ".\roms" `
  --bios-path ".\bios"
```

The existing SDL2 build has been manually tested for windowed startup, menus, gameplay, audio, and USB gamepad input. These results apply to that specific build and do not automatically validate other build directories or future versions.

## Game data

ROM and BIOS files are not included in this repository. Place legally obtained files in the local directories:

```text
magdrop3/roms/
magdrop3/bios/
```

The GUI supplies the ROM and BIOS directory paths when launching the executable.

Build-time recompilation settings are separate from runtime paths. A blank `MAGDROP3_ROM_DIR` or `MAGDROP3_BIOS_DIR` value in the CMake cache means that the corresponding path was not configured for that build-time step; it does not necessarily indicate a problem with runtime paths supplied by the launcher.

Keep game data separate from source code and generated build outputs. Never commit ROM or BIOS files.

## Controller configuration

The GUI supports controller binding capture and saving controller assignments locally.

The local `magdrop3/controls.cfg` file is excluded from version control to avoid committing personal settings. Controller capture requires the SDL2-enabled executable.

## Dependency and attribution

The Neo Geo recompilation toolkit originated in the project maintained by `sp00nznet`:

https://github.com/sp00nznet/neogeorecomp

The local toolkit repository is maintained separately from the root `Magdrop3PC` repository and contains project-specific changes, including SDL2 and controller-capture work. Changes to the toolkit belong to the nested toolkit repository's history, not the root repository's commit history.

The `neogeorecomp/` submodule records a specific toolkit commit. Updating the toolkit requires updating the submodule checkout and, when appropriate, committing the new submodule reference in the root repository.

Preserve upstream license and copyright notices and notices for bundled dependencies. Before distributing a fresh setup intended for other users, verify that all dependency references are accessible and that the required build instructions and licensing information are complete.

`metalslug-reference/` and `vcpkg/` are local-only checkouts at present and are not automatically populated by cloning the root repository.

## Version-control exclusions

The root `.gitignore` excludes:

- ROM and BIOS directories
- Build output directories and compiled binaries
- Generated screenshots and verification logs
- Local controller settings
- Other local or generated files intended to remain outside version control

Review staged files before every commit. Never commit ROMs, BIOS dumps, generated build artifacts, or personal controller settings.

The toolkit has its own Git repository and working-tree status. Check it separately when reviewing toolkit changes:

```powershell
git status --short --branch
git -C .\neogeorecomp status --short --branch
git submodule status
```

A clean root repository does not by itself guarantee that the nested toolkit working tree is clean.

## Development notes

- Keep experimental builds in separate directories.
- Do not reuse a CMake build directory after changing generators or toolchains; create a new build directory instead.
- Run conformance tests and recompilation verification after changes to the toolkit.
- Record verification results with the specific build configuration and command used.
- Treat a successful headless verification run as evidence for that run, not as proof that every gameplay scenario has been tested.
- Test newly compiled executables separately before replacing the known-good executable.
