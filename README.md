# Magdrop3PC

A Windows-focused recompilation project for the MVS arcade version of
Magical Drop III, built on the Neo Geo recompilation toolkit.

The project aims to keep game-specific code separate from the underlying
Neo Geo runtime and to support enhancements and modding.

## Project structure

- `magdrop3/` — Magical Drop III game-specific source, tests, and CMake configuration.
- `MagicalDropGUI.ps1` — Windows GUI for launching the game and configuring controls.
- `MagicalDropGUI.vbs` — Windows launcher for the GUI.
- `neogeorecomp/` — Local checkout of the Neo Geo recompilation toolkit.
- `metalslug-reference/` — Local reference project.
- `vcpkg/` — Local C/C++ dependency manager checkout.

The three sibling repositories are managed independently from this repository.
They are excluded from the root repository for now.

## Requirements

- Windows
- CMake
- Visual Studio C++ build tools
- SDL2 for the windowed game and controller-capture functionality
- vcpkg for dependency management, as configured in the local build

Exact build settings may depend on the local toolchain and dependency paths.

## Local dependency setup

The current CMake configuration expects the `neogeorecomp` checkout beside
the `magdrop3` directory:

```text
Magdrop3PC/
├── magdrop3/
├── neogeorecomp/
├── metalslug-reference/
├── vcpkg/
├── MagicalDropGUI.ps1
└── MagicalDropGUI.vbs
```

`magdrop3/CMakeLists.txt` uses `../neogeorecomp` by default.

The toolkit is currently maintained as a separate local Git repository.
A portable dependency reference, such as a Git submodule pointing to a
project-maintained fork, remains a future setup task.

## Game data

ROM and BIOS files are not included in this repository. Place legally obtained
game data in the local `magdrop3/roms/` and `magdrop3/bios/` directories.

Build outputs, generated files, and local controller configuration are also
excluded from version control.

## Development notes

- Keep game-specific changes in `magdrop3/` where practical.
- Keep toolkit changes in `neogeorecomp/` and commit them in that repository.
- Review staged files before committing.
- Do not commit ROMs, BIOS dumps, build outputs, or personal configuration.
