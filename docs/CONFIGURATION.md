# Runtime Data and Configuration

This document describes the files used by a built regional executable and the settings saved by the SDL2 runtime.

## Runtime directory layout

The game resolves its default runtime data paths relative to the executable, not the current working directory.

    Build-<Region>/
    ├── magdrop3.exe
    ├── SDL2.dll
    ├── settings.cfg
    ├── rom/
    └── bios/

- **rom/** contains uncompressed Magical Drop III MVS game ROM files.
- **bios/** contains uncompressed Neo Geo system ROM files required by the selected regional configuration.
- **settings.cfg** is created/updated by the game to persist controls, graphics and sound settings.
- **SDL2.dll** is present when required by the Windows SDL2 build.

CMake creates the empty rom/ and bios/ folders beside the executable. Users must supply their own compatible files.

## Required game data

The game definition in magdrop3/src/game.c describes these game ROM filenames:

| ROM role | Expected filename |
|---|---|
| Program | 233-p1.bin |
| Fix layer | 233-s1.bin |
| Audio program | 233-m1.bin |
| Character graphics | 233-c1.bin, 233-c2.bin, 233-c3.bin, 233-c4.bin |
| Video/audio sample data | 233-v1.bin, 233-v2.bin |

The Neo Geo system set includes files such as:

- **sp-s2.sp1** — MVS Asia/Europe v2 BIOS; the actual BIOS filename is selected by the regional build configuration.
- **sfix.sfix**
- **sm1.sm1**
- **000-lo.lo** — sprite shrink table.

Use the game set and system files that match the selected region and the build's BIOS configuration. The runtime expects uncompressed files; ZIP archives are not read directly.

## Command-line overrides

The runtime supports these options:

| Option | Purpose |
|---|---|
| --rom-path DIR | Override the default game ROM directory |
| --bios-path DIR | Override the default system BIOS directory |
| --bios FILE | Override the configured system BIOS filename |

By default, the executable looks for rom/ and bios/ beside itself. For backward compatibility, if --rom-path is explicitly supplied without --bios-path, the BIOS directory defaults to the supplied ROM directory.

The regional BIOS filename can also be selected at build time through the CMake MAGDROP3_BIOS_FILE variable. The default is sp-s2.sp1; regional builds should use their corresponding configured filename.

## settings.cfg

The SDL2 runtime stores settings.cfg beside the executable using SDL's executable base path. Each regional build should therefore have its own settings file.

The file is divided into three sections:

### [Controls]

Contains keyboard bindings, controller bindings and saved controller identity information.

Keyboard actions include both player controls and system actions such as overlay, fullscreen, pause and screenshot. Controller bindings can represent:

- **BTN<n>** — joystick button index.
- **HAT<n>_UP**, **HAT<n>_DOWN**, **HAT<n>_LEFT**, **HAT<n>_RIGHT** — hat direction.
- **AXIS<n>_NEG** or **AXIS<n>_POS** — negative or positive axis direction.
- **NONE** — no explicit binding / automatic mapping where supported.

The runtime also saves controller GUID, serial and path information when available so it can identify the devices assigned to each player.

### [Graphics]

The current graphics settings include:

| Key | Values | Meaning |
|---|---|---|
| WINDOW_SIZE | 2X to 5X | Window scale |
| ASPECT_RATIO | ORIGINAL, 4:3, 16:9 | Logical canvas/aspect option |
| FULLSCREEN | ON, OFF | Desktop fullscreen toggle |
| VSYNC | ON, OFF | Vertical synchronization |
| FILTER | NEAREST, BILINEAR, SCANLINES | Texture scaling / scanline effect |

The in-game menu labels the scanline filter as CRT. The current implementation draws alternating horizontal dark lines; it is not a full CRT shader.

### [Sound]

| Key | Values | Meaning |
|---|---|---|
| MASTER_VOLUME | 0 to 100, steps of 5 | Master volume |
| MUTE | ON, OFF | Mute toggle |

## Editing settings manually

Prefer changing settings through the in-game overlay. If editing the file manually:

1. Close the game first.
2. Make a backup of settings.cfg.
3. Preserve the section headers and key/value format.
4. Use values from the tables above.
5. Start the game and verify the setting.

Invalid or duplicate keyboard assignments may be rejected and replaced with defaults. Unknown or invalid values should not be assumed to be preserved.

## Generated entries file

When the launcher's **Record misses for next build** option is enabled, missing recompiler entry points are recorded in the selected region's entries.txt. The runtime merges new entries with existing ones, removes duplicates and sorts the result when the game exits.

This file is a development aid for the recompiler workflow. It is not a substitute for the original ROM/BIOS data and should remain associated with the regional build that produced it.
