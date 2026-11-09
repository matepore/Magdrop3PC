# Keyboard and Controller Input

Input configuration is available from the in-game overlay (default overlay key: **F1**) under **Controls**.

The SDL2 backend supports keyboard input, SDL-mapped game controllers and generic SDL joysticks. The runtime maintains an inventory of up to eight connected physical input devices and assigns devices to the two game players.

## Default keyboard bindings

The defaults in the current SDL2 backend are:

| Action | Player 1 | Player 2 |
|---|---|---|
| Up | Up Arrow | W |
| Down | Down Arrow | S |
| Left | Left Arrow | A |
| Right | Right Arrow | D |
| A | Z | F |
| B | X | G |
| C | C | H |
| D | V | J |
| Start | 1 | 2 |
| Coin | 5 | 6 |

Additional system actions:

| Action | Default key |
|---|---|
| Select | 3 |
| Service | 9 |
| Test switch | F2 |
| Open/close overlay | F1 |
| Toggle fullscreen | F11 |
| Pause/resume | Pause |
| Screenshot | Print Screen |
| Quit | Escape |

These are defaults, not fixed requirements. Keyboard bindings can be changed in the overlay and are saved in settings.cfg.

## Controller support

The runtime supports SDL GameController mappings and generic SDL joystick devices. A controller can be connected while the game is running. The overlay provides separate controller configuration screens for Player 1 and Player 2.

Controller actions can be bound to buttons, hat directions or axis directions. The saved representation in settings.cfg uses values such as:

- **BTN0** — button 0.
- **HAT0_UP** — up on hat 0.
- **AXIS0_NEG** — negative direction on axis 0.
- **AXIS0_POS** — positive direction on axis 0.
- **NONE** — automatic/default mapping where supported.

The overlay also supports changing the controller assigned to a player, restoring defaults and resolving binding conflicts.

## Device identity and reconnection

The runtime records available device identity information (GUID and, when supported by the SDL version/device, serial number and device path). This information helps it identify the physical controller associated with each player when devices are connected again.

If a controller is not recognized as an SDL GameController, it can still be handled through SDL's lower-level joystick interface. Its buttons, hats and axes can be assigned manually through the overlay.

## Configuration persistence

Keyboard bindings, controller bindings and controller identity information are saved in the [Controls] section of settings.cfg, located beside the executable.

Use the overlay to change bindings. If editing the file manually, close the game first and keep a backup.
