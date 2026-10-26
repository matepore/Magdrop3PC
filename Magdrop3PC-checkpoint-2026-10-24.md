# Magdrop3PC — Stable Build and Technical Checkpoint

**Checkpoint date:** October 24, 2026
**Platform:** Windows x64
**Game:** Magical Drop III — MVS arcade version
**Stable runtime:** SDL2-enabled build

## 1. Project goal

Magdrop3PC is a Windows-focused static recompilation of the MVS arcade version of *Magical Drop III*.

The goal is to keep game-specific code separate from the Neo Geo runtime and game data, making future modifications and enhancements easier to maintain.

ROM and BIOS files are local user-provided assets. They must remain separate from source code and must not be committed to Git.

## 2. Repository layout

Root repository:

`C:\Otros\Repositorios\Magdrop3PC`

Important paths:

- `magdrop3\` — Game-specific source code, tests, and CMake configuration.
- `neogeorecomp\` — Neo Geo recompilation toolkit, tracked as a Git submodule pinned to a specific commit.
- `metalslug-reference\` — Local reference project maintained separately from the root repository.
- `vcpkg\` — Local vcpkg checkout used to manage C/C++ dependencies.
- `magdrop3\roms\` — Local game ROM files.
- `magdrop3\bios\` — Local Neo Geo BIOS files.
- `magdrop3\controls.cfg` — Local controller configuration.
- `magdrop3\build-sdl2\` — Preserved SDL2 build.
- `magdrop3\build-gameplay\` — Separate build used for headless gameplay verification.

Do not delete or overwrite existing build directories as part of routine cleanup. In particular, preserve `magdrop3\build` and `magdrop3\build-sdl2`.

The `neogeorecomp` directory is a Git submodule. Its recorded commit is part of the root repository's history. The `metalslug-reference` and `vcpkg` directories are local dependencies and are not automatically populated by cloning the root repository.

## 3. Stable SDL2 runtime

The known working executable is:

`magdrop3\build-sdl2\magdrop3.exe`

The launcher is:

`magdrop3\Execute_Magdrop3_SDL2.bat`

The launcher and executable have been manually tested. Confirmed functionality includes:

- Opening the SDL2 game window.
- Loading the game with the local ROM and BIOS directories.
- Navigating menus and entering gameplay.
- Audio playback.
- USB gamepad detection and input.
- Loading controller settings from `controls.cfg`.

Run the launcher from the project setup rather than assuming that launching the executable from an arbitrary working directory will find every local configuration file.

The game looks for `controls.cfg` relative to the current working directory. The launcher should switch to its own directory before starting the executable. If the game is started from another directory, it may not find the local configuration and may fall back to default controls.

The local `magdrop3\controls.cfg` file contains personal controller settings and must not be committed.

A direct invocation from the `magdrop3` directory can use:

```powershell
.\build-sdl2\magdrop3.exe `
  --rom-path ".\roms" `
  --bios-path ".\bios"
```

Keep the stable SDL2 build separate from experimental builds. A successful headless verification run does not, by itself, prove that the SDL2 runtime has been tested.

## 4. Recompilation and verification

The `neogeorecomp` runtime contains a fix in `src\exec.c` that explicitly sequences the two 16-bit bus reads used by a 32-bit read. This avoids relying on unspecified C expression evaluation order.

The relevant implementation is:

```c
static inline uint32_t ng_r32(uint32_t a) {
    uint16_t hi = ng_r16(a);
    uint16_t lo = ng_r16(a + 2);
    return ((uint32_t)hi << 16) | lo;
}
```

Mismatch diagnostics were also improved to include the interpreter program counter.

### Latest recorded headless verification

The separate `magdrop3\build-gameplay` build completed a 100,000-frame verification run with:

- **Recompiled entry points:** 14,092
- **Blocks checked:** 45,359,098
- **Mismatches:** 0
- **Native blocks:** 45,359,098
- **Interpreted instructions:** 0
- **Uncovered entry points:** 0

These results apply to that specific headless run and build configuration. They do not guarantee that every possible game state or runtime scenario has been tested.

Record the build directory, command, and verification output when repeating this test so that results from different builds are not confused.

## 5. Build environment

The Windows build environment uses CMake, Visual Studio C++ Build Tools, SDL2, and vcpkg.

The local vcpkg toolchain file is:

`C:\Otros\Repositorios\Magdrop3PC\vcpkg\scripts\buildsystems\vcpkg.cmake`

The documented SDL2 build configuration uses the `x64-windows` triplet. Build directories are kept separate to avoid mixing incompatible CMake caches or experimental configurations.

The original build setup used the NMake Makefiles generator with MSVC x64. The README also documents a Visual Studio generator configuration. These are separate configurations: do not reuse a build directory after changing its CMake generator, compiler, architecture, or toolchain. Use a separate build directory for each configuration.

The installed CMake version recorded during setup was `4.5.0-rc1`, a release candidate rather than a stable release. Other environments may require different tool versions or generator settings.

SDL2 must be available for the SDL2-enabled runtime. Do not assume that every experimental or headless build has SDL2 enabled.

The root README contains the current build instructions. Recheck the selected generator and installed tool versions before reproducing the build on another machine. Test a newly compiled executable before treating it as equivalent to the known working build.

## 6. Git repositories and status

### Root repository

- **Repository:** https://github.com/matepore/Magdrop3PC.git
- **Branch:** `master`
- **Latest verified commit:** `5f1fb19` — `Update neogeorecomp with bus read ordering fix`
- **Remote:** `origin` points to the repository URL above.
- The branch was reported synchronized with `origin/master` at the last status check.

### Neo Geo recompilation toolkit

- **Local path:** `neogeorecomp\`
- **Repository:** https://github.com/matepore/neogeorecomp.git
- **Upstream project:** https://github.com/sp00nznet/neogeorecomp
- **Branch:** `master`
- **Latest verified commit:** `808460c` — `Fix ordering of 32-bit bus reads`
- **Remote:** `origin` points to the `matepore/neogeorecomp` repository.
- The branch was reported synchronized with `origin/master` at the last status check.
- The root repository records submodule commit `808460ca83a1be2a44be0e255e2756e6ea736744`.

### Pending local changes

The latest root status check showed:

```text
## master...origin/master
 M Magdrop3PC-checkpoint-2026-10-24.md
 M README.md
 ? neogeorecomp
```

The root repository has changes to the README and this checkpoint. Git also reports a submodule working-tree condition for `neogeorecomp`.

The submodule index entry is:

```text
160000 808460ca83a1be2a44be0e255e2756e6ea736744 0 neogeorecomp
```

The toolkit checkout's `HEAD` matches that recorded commit. Its latest reported status was:

```text
## master...origin/master
?? src/exec.c.before-first-mismatch.bak
```

The backup file is an untracked copy of an earlier version of `src\exec.c`'s mismatch diagnostic. Unlike the current implementation, that earlier diagnostic does not include the interpreter program counter.

The backup is being retained temporarily for reference. Do not commit it automatically or remove it without first deciding whether it is still useful.

The nested dependencies `ext\musashi`, `ext\ymfm`, and `ext\z80` were checked recursively and showed no local changes in their reported statuses. Their detached `HEAD` states are normal for pinned submodule dependencies.

Useful commands for reviewing repository status are:

```powershell
git status --short --branch
git -C .\neogeorecomp status --short --branch
git submodule status --recursive
git ls-files --stage -- neogeorecomp
```

To inspect the root documentation changes:

```powershell
git diff -- README.md
git diff -- Magdrop3PC-checkpoint-2026-10-24.md
```

To inspect toolkit changes independently:

```powershell
git -C .\neogeorecomp diff
git -C .\neogeorecomp status --short --branch
```

Review the files intended for each commit before staging them. Never commit ROMs, BIOS files, generated binaries, build outputs, or personal controller settings.

Keep root repository changes separate from changes belonging to the nested toolkit repository. A root commit can record a new submodule reference, but it does not include the toolkit's source changes as ordinary root-repository files.

## 7. Recommended next steps

1. Preserve the known working SDL2 build and launcher.
2. Keep the root README and this checkpoint consistent and maintain project documentation in English.
3. Review the final documentation changes before staging or committing.
4. Retain the `src\exec.c.before-first-mismatch.bak` file until its usefulness has been decided.
5. Investigate the reported submodule working-tree condition only if it prevents a clean and understandable repository status.
6. Make future runtime or recompilation changes in a separate, testable step.
7. Re-run relevant verification after code changes and record the exact build directory, command, and results.
8. Test newly compiled executables separately before replacing the known working executable.
