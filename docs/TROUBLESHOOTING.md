# Troubleshooting

This guide covers common setup, build and runtime issues. It does not replace the launcher checks or error messages printed by the runtime.

## The game reports a missing ROM

1. Confirm that the files are uncompressed; ZIP archives are not read directly.
2. Confirm that the files are in the rom/ directory beside the running magdrop3.exe.
3. Confirm that the files belong to the Magical Drop III MVS set and use the filenames expected by the game definition.
4. Make sure you are checking the directory beside the executable that you actually launched, not a similarly named source or build directory.

The runtime resolves the default data path relative to the executable, not the current working directory.

## The game reports a missing BIOS/system ROM

1. Confirm that the uncompressed system files are in the bios/ directory beside the executable.
2. Confirm that the BIOS filename matches the regional build configuration.
3. Confirm that the system files required by the runtime are present (including the fix-layer, sound-program and sprite-shrink data where required).
4. If using command-line overrides, check both --bios-path and --bios.

The default BIOS filename is sp-s2.sp1 (MVS Asia/Europe), but other regional builds may select a different filename. Do not assume that the default is correct for every region.

## The game starts from a shortcut but cannot find data

The default ROM and BIOS paths are resolved relative to the executable. Place rom/ and bios/ beside the actual executable being launched. Do not rely on the shortcut's Start in directory.

If you intentionally use custom locations, pass the supported --rom-path and --bios-path arguments.

## SDL2 or window initialization fails

- Confirm that the build was configured with SDL2 available through the intended vcpkg installation.
- Confirm that SDL2 was installed for the x64-windows triplet used by the build.
- For a Windows build, check that the required SDL2 runtime DLL is beside the executable. The CMake post-build step copies the SDL2 runtime DLLs when the SDL2 target is available.
- If the project was built without SDL2, the toolkit runtime is headless-only and does not provide the normal game window.

The runtime attempts to create an accelerated renderer and falls back to SDL's software renderer if the accelerated renderer cannot be created.

## Graphics setting does not apply

- Reopen the Graphics menu and check the displayed value.
- If changing VSync fails, the runtime reports the SDL error and reverts the setting.
- Some scaling behavior depends on the desktop resolution and the active SDL renderer.
- The CRT option currently means scanlines, not a complete CRT shader.

## Settings unexpectedly return to defaults

- Confirm that settings.cfg is beside the executable you are running.
- Confirm that the directory is writable.
- If the file was edited manually, check its section names and key/value syntax.
- Invalid or duplicate keyboard bindings may cause the runtime to restore default keyboard bindings.

Each regional executable should keep its own settings file.

## Controller is not detected or does not respond

- Connect the controller before or while the game is running.
- Check the controller assignment in the Controls overlay for the intended player.
- If SDL does not recognize the device as a GameController, use the generic controller configuration and bind its buttons, hats or axes manually.
- Check that the device has not been assigned to the other player.
- If a binding is incorrect, capture it again or restore the controller defaults.

## Where to find diagnostic information

The current runtime prints errors and status messages to standard output/error, including SDL initialization failures, settings write failures, missing game data and screenshot errors.

A persistent diagnostic log file is not currently part of the project. If a problem needs investigation, record the exact message, selected region, Windows version, build configuration and steps that reproduce it.
