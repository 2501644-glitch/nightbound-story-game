@echo off
call "%~dp0build_game.bat"
if %ERRORLEVEL% EQU 0 (
    call "%~dp0run_game.bat"
)
