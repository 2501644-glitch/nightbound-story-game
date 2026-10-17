@echo off
echo ========================================================
echo   NIGHTBOUND - Launching Standalone Game
echo ========================================================

set EXE_PATH="%~dp0Builds\Nightbound.exe"

if not exist %EXE_PATH% (
    echo [ERROR] %EXE_PATH% not found!
    echo Please run build_game.bat first.
    exit /b 1
)

start "" %EXE_PATH%
echo [SUCCESS] Nightbound launched!
