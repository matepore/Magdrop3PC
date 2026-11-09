# Graphics and Display

The graphics options are available from the in-game overlay (default overlay key: F1), under **Graphics**.

## Window size

Windowed mode supports scale selections from **2x through 5x**. The selected scale is applied to the logical canvas dimensions.

The selected window scale is stored as WINDOW_SIZE in the [Graphics] section of settings.cfg.

## Aspect ratio options

The graphics menu offers:

- **Original** — uses the Neo Geo framebuffer dimensions as the logical canvas.
- **4:3** — uses a 320 × 240 logical canvas.
- **16:9** — uses a 448 × 252 logical canvas. The game image is drawn in a centered 336 × 252 rectangle, preserving a 4:3 game area with space on the sides.

The 16:9 option changes the presentation canvas; it does not stretch the game image to 16:9.

## Fullscreen and VSync

Fullscreen uses SDL's SDL_WINDOW_FULLSCREEN_DESKTOP mode. This is desktop fullscreen rather than an exclusive display-mode switch.

VSync is controlled through SDL's renderer API. If the active renderer cannot enable the requested VSync mode, the runtime reports the SDL error and reverts the setting rather than claiming the change succeeded.

The renderer is created with the accelerated SDL renderer when available, with a software-renderer fallback.

## Current filters

### Nearest Neighbor

Uses SDL's nearest-neighbor texture scaling. It preserves hard pixel edges and is generally the best starting point when the intention is to retain the original pixel-art appearance.

### Bilinear

Uses SDL's linear texture scaling. It blends neighboring pixels during scaling and can appear smoother, but may soften pixel edges.

### CRT (scanlines)

The current CRT option is a lightweight scanline overlay. It draws a semi-transparent dark horizontal line on alternating rows of the game rectangle.

This is not a full CRT simulation: the current implementation does not include phosphor masks, glow, curvature or analog signal effects. Those would require additional rendering work and are not part of the current feature set.

The menu's CRT filter is stored as FILTER=SCANLINES in settings.cfg.

## Scaling behavior and limitations

The runtime sets an SDL logical rendering size and draws the game texture into a destination rectangle. Windowed dimensions are calculated from the logical canvas and the selected 2x–5x scale.

The current code does not explicitly enable SDL's integer-scale mode through SDL_RenderSetIntegerScale(). Consequently, fullscreen output may use a fractional scale depending on the desktop resolution and renderer. If uniform pixel sizes are important, this behavior should be evaluated separately before changing the default.

## Future graphics work under consideration

The following are investigation items, not implemented features:

- HQx image scaling.
- Scale2x image scaling.
- Optional integer scaling for fullscreen presentation.
- A dedicated **Customize CRT** submenu, enabled only while CRT is selected. The initial scope is limited to effects that can be implemented using the existing SDL2 renderer without introducing another dependency. Glow and curvature are excluded if they require a new rendering backend or external dependency.
- A small optional FPS/frame-time display.

Any new filter should be compared against the actual game image, checked for artifacts and measured for CPU/GPU cost before being added to the menu.
