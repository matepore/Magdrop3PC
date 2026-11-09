# Building Magdrop3PC

This guide covers the Windows x64 development setup and regional build flow. The project does not publish prebuilt game builds or include game ROM/BIOS data.

## Preferred workflow: launcher

The repository includes MagicalDropLauncher.exe in its root directory. The launcher is the intended user-facing workflow:

1. Clone the repository with its submodules.
2. Provide compatible, uncompressed Magical Drop III ROM files and required Neo Geo BIOS files.
3. Open MagicalDropLauncher.exe.
4. Select the desired game region.
5. Use the launcher's dependency and game-data checks to resolve missing prerequisites.
6. Use the launcher's build action for the selected region.
7. Start the resulting executable from the launcher or run it directly.

Builds are initiated by the user through the launcher. This project does not require or provide an automatic cloud build/release workflow.

The launcher executable is included in the repository, but its source is maintained locally and is not part of this repository.

## Clone and initialize submodules

From PowerShell:

    git clone --recurse-submodules https://github.com/matepore/Magdrop3PC.git
    cd Magdrop3PC

If the repository was cloned without submodules:

    git submodule update --init --recursive

The neogeorecomp submodule must be initialized before configuring the game project.

## Development dependencies

The documented manual Windows x64 build uses:

- Git, including submodule support
- CMake 3.21 or newer
- Visual Studio C++ Build Tools with MSVC and the Windows SDK
- NMake, available in a Visual Studio developer environment
- vcpkg
- SDL2 installed through vcpkg for the x64-windows triplet

SDL2 provides the windowed runtime. If CMake does not find SDL2, the toolkit can build without its SDL2 window backend (headless-only); that is not the intended configuration for the normal desktop game.

## Set up vcpkg and SDL2

If vcpkg is not already available, clone and bootstrap it from the repository root:

    git clone https://github.com/microsoft/vcpkg.git
    .\vcpkg\bootstrap-vcpkg.bat

Install SDL2 for Windows x64:

    .\vcpkg\vcpkg.exe install sdl2:x64-windows

If vcpkg is installed elsewhere, use its actual path in the CMake toolchain argument below.

## Manual regional build with CMake

The launcher is preferred, but the game can also be configured and built manually.

Open Developer PowerShell for Visual Studio with the x64 MSVC environment available. Use a separate build directory for each region so that CMake cache values and generated game code do not get mixed between regional configurations.

Example for an Asia/Europe configuration (the default system BIOS filename is sp-s2.sp1):

    $romDir = "$PWD\rom"
    $biosDir = "$PWD\bios"
    $toolchain = "$PWD\vcpkg\scripts\buildsystems\vcpkg.cmake"

    cmake -S magdrop3 -B magdrop3/build-asia-europe -G "NMake Makefiles" -DCMAKE_BUILD_TYPE=Release -DCMAKE_TOOLCHAIN_FILE="$toolchain" -DVCPKG_TARGET_TRIPLET=x64-windows -DMAGDROP3_ROM_DIR="$romDir" -DMAGDROP3_BIOS_DIR="$biosDir" -DMAGDROP3_BIOS_FILE=sp-s2.sp1

    cmake --build magdrop3/build-asia-europe --target magdrop3

For USA and Japan, use their corresponding regional BIOS selection and a separate build directory. Do not assume that the Asia/Europe BIOS filename is correct for another region; use the regional configuration selected by the project/launcher.

### Relevant CMake cache variables

| Variable | Purpose |
|---|---|
| MAGDROP3_ROM_DIR | Directory containing source game ROM files used by the recompiler |
| MAGDROP3_BIOS_DIR | Directory containing Neo Geo system ROM files used during recompilation |
| MAGDROP3_BIOS_FILE | System BIOS filename selected for this build; defaults to sp-s2.sp1 |
| MAGDROP3_ENTRIES | Additional recompiler entry-point list; defaults to entries.txt in the build directory |
| NEOGEORECOMP_DIR | Path to the toolkit checkout; defaults to the neogeorecomp directory beside magdrop3 |

When MAGDROP3_ROM_DIR is set, CMake configures the recompiler generation step and includes generated native sources in the game executable. Without it, the game target is built without the generated recompiled function table.

### Output and runtime folders

CMake's post-build step creates empty rom/ and bios/ directories beside the resulting magdrop3.exe. On Windows, when the SDL2 target is available, CMake also copies the required SDL2 runtime DLLs beside the executable.

The runtime data directories are separate from the source directories passed to MAGDROP3_ROM_DIR and MAGDROP3_BIOS_DIR. Users must supply compatible runtime files in the rom/ and bios/ folders next to the executable.

## Regional build isolation

Keep a separate build directory and executable/data directory for each region. This avoids accidentally reusing:

- a CMake cache configured for another BIOS,
- generated C files from another regional ROM set,
- a different entry-point list,
- local settings from another regional executable.

The project has three supported regional configurations: Europe/Asia, USA, and Japan. Verify each region using its matching game data and BIOS configuration.

## What must not be committed

Do not commit or publish:

- game ROM files,
- Neo Geo BIOS files,
- generated C sources or other outputs derived from copyrighted game data,
- local build directories or CMake caches,
- local user settings unless there is a specific reason and the contents have been reviewed.

The repository's source license does not grant rights to game code or system ROMs. Users must provide their own compatible data.
