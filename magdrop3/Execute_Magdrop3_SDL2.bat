
@echo off
setlocal
title Magical Drop III - SDL2

rem Use this folder as the working directory.
cd /d "%~dp0"

set "EXE=%~dp0build-final\magdrop3.exe"
set "ROM_DIR=%~dp0roms"
set "BIOS_DIR=%~dp0bios"

echo ==========================================
echo       MAGICAL DROP III - SDL2
echo ==========================================
echo.

if not exist "%EXE%" (
    echo ERROR: No se encontro el ejecutable:
    echo "%EXE%"
    goto error
)

if not exist "%ROM_DIR%" (
    echo ERROR: No se encontro la carpeta de ROMs:
    echo "%ROM_DIR%"
    goto error
)

if not exist "%BIOS_DIR%" (
    echo ERROR: No se encontro la carpeta de BIOS:
    echo "%BIOS_DIR%"
    goto error
)

echo Iniciando el juego...
echo Los controles se cargaran desde controls.cfg.
echo.

"%EXE%" --rom-path "%ROM_DIR%" --bios-path "%BIOS_DIR%"
set "RC=%ERRORLEVEL%"

echo.
echo El juego termino con codigo de salida: %RC%

if not "%RC%"=="0" goto error

echo.
echo Ejecucion finalizada.
pause
exit /b 0

:error
echo.
echo Se produjo un error o falta un archivo.
echo Revisa los mensajes anteriores.
pause
exit /b 1
