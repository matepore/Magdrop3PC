# Magdrop3PC --- Checkpoint técnico estable

**Fecha:** 24 de octubre de 2026\
**Estado:** ejecución SDL2 funcional; checkpoint de referencia antes de
nuevas modificaciones.

## 1. Objetivo

Recompilación de *Magical Drop III* para Windows, versión arcade MVS,
manteniendo separados el código y los datos del juego para facilitar
futuras mejoras y modding.

## 2. Rutas principales

-   Raíz del repositorio: `C:\Otros\Repositorios\Magdrop3PC`
-   Carpeta del juego: `C:\Otros\Repositorios\Magdrop3PC\magdrop3`
-   Ejecutable SDL2 funcional: `magdrop3\build-sdl2\magdrop3.exe`
-   ROMs: `magdrop3\roms`
-   BIOS: `magdrop3\bios`
-   Controles personalizados: `magdrop3\controls.cfg`
-   Lanzador: `magdrop3\Ejecutar_Magdrop3_SDL2.bat`
-   Build anterior: `magdrop3\build` (conservar; no reemplazar ni
    borrar)

## 3. Estado funcional confirmado

El usuario confirmó que el juego abre y funciona correctamente cuando se
inicia desde la carpeta `magdrop3`.

-   La ventana SDL2 abre correctamente.
-   ROMs y BIOS cargan.
-   El audio funciona.
-   SDL2 detecta el mando `USB Gamepad`.
-   Los controles personalizados de `controls.cfg` funcionan.
-   El lanzador `.bat` funciona correctamente.
-   La consola mantiene visibles los mensajes de inicio y salida.

### Último registro compartido

``` text
[input] Player 1 connected: USB Gamepad (SDL game controller)
[neogeorecomp] magdrop3: 12983 recompiled entry points
[exec] native blocks 2957520, interpreted instructions 5409, 64 uncovered entry points
```

En una prueba anterior apareció:

``` text
[input] controls.cfg missing; using defaults.
```

La causa fue el directorio de trabajo. SDL2 busca `controls.cfg`
relativo al directorio actual; ejecutar el juego desde `magdrop3`
permite encontrarlo. Si se inicia desde otra carpeta, puede usar los
controles predeterminados.

## 4. Cómo ejecutar la versión estable

Ejecutar:

`C:\Otros\Repositorios\Magdrop3PC\magdrop3\Ejecutar_Magdrop3_SDL2.bat`

El lanzador establece la carpeta de trabajo con:

``` bat
cd /d "%~dp0"
```

y ejecuta el programa con las rutas de ROMs y BIOS.

### Ejecución manual equivalente

Desde CMD:

``` bat
cd /d C:\Otros\Repositorios\Magdrop3PC\magdrop3
build-sdl2\magdrop3.exe --rom-path "C:\Otros\Repositorios\Magdrop3PC\magdrop3\roms" --bios-path "C:\Otros\Repositorios\Magdrop3PC\magdrop3\bios"
```

## 5. Configuración de compilación SDL2

-   Generador de CMake: `NMake Makefiles`
-   Compilador: MSVC x64 desde Visual Studio Build Tools
-   Toolchain de vcpkg:
    `C:\Otros\Repositorios\Magdrop3PC\vcpkg\scripts\buildsystems\vcpkg.cmake`
-   Triplet: `x64-windows`
-   Paquete SDL2:
    `C:\Otros\Repositorios\Magdrop3PC\vcpkg\installed\x64-windows\share\sdl2`
-   Directorio de build separado: `magdrop3\build-sdl2`

La compilación SDL2 se mantuvo separada de la build anterior para no
sobrescribirla.

## 6. Repositorios y estado Git conocido

-   Repositorio principal: `https://github.com/matepore/Magdrop3PC.git`
-   Submódulo: `neogeorecomp`
-   Fork del submódulo: `https://github.com/matepore/neogeorecomp.git`
-   Upstream: `https://github.com/sp00nznet/neogeorecomp.git`

Último estado Git conocido antes de esta prueba SDL2: - El repositorio
principal estaba limpio en la última comprobación registrada. -
`neogeorecomp` estaba limpio y sincronizado con `origin/master`, commit
`fe07115` (`Add SDL2 controller capture support`). - Los submódulos
externos de `neogeorecomp` estaban inicializados.

**Este estado Git es histórico**, no una comprobación del estado actual
tras crear/probar el lanzador y la build SDL2. Verificar antes de hacer
commit.

## 7. Verificar el estado actual antes de modificar

Abrir CMD y ejecutar:

``` bat
cd /d C:\Otros\Repositorios\Magdrop3PC
git status --short
git -C neogeorecomp status --short
git -C neogeorecomp log -1 --oneline
```

Para comprobar si los archivos están ignorados:

``` bat
git check-ignore -v magdrop3\controls.cfg magdrop3\Ejecutar_Magdrop3_SDL2.bat magdrop3\build-sdl2
```

`git check-ignore` puede devolver código distinto de cero si alguno no
está ignorado; eso no es necesariamente un error. No agregar
automáticamente ROMs, BIOS, ejecutables ni archivos generados al
repositorio.

## 8. Precauciones para futuras tareas

1.  Verificar el estado Git antes de modificar código o configuración.
2.  No borrar ni reemplazar `magdrop3\build` ni `magdrop3\build-sdl2`.
3.  No modificar ROMs, BIOS ni `controls.cfg` en tareas que no lo
    requieran.
4.  Mantener la build SDL2 separada.
5.  Probar el juego mediante el `.bat` después de cambios que puedan
    afectar el arranque.
6.  Antes de modificar código, realizar un commit o una copia de
    seguridad explícita.

## 9. Tareas futuras posibles (no iniciadas)

-   Documentar los controles y la configuración del entorno en el
    README.
-   Investigar los `64 uncovered entry points` y las
    `5409 interpreted instructions`, sin asumir que sean fallos.
-   Evaluar mejoras futuras para configuración de mandos o distribución
    para Windows.

Estos son trabajos futuros; el juego ya funciona y no hace falta
abordarlos para considerar estable este checkpoint.
