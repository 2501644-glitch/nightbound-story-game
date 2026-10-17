@echo off
setlocal

set "GAME_EXE=%~dp0Builds\Nightbound.exe"

if not exist "%GAME_EXE%" (
    echo [ERROR] Game not found: "%GAME_EXE%"
    echo Run build_game.bat first.
    endlocal & exit /b 1
)

start "" "%GAME_EXE%"
endlocal & exit /b 0
