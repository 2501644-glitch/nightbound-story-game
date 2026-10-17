@echo off
call "%~dp0build_game.bat"
if errorlevel 1 exit /b %ERRORLEVEL%

call "%~dp0run_game.bat"
exit /b %ERRORLEVEL%
