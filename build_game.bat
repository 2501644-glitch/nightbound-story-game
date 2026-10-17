@echo off
echo ========================================================
echo   NIGHTBOUND - Building via Unity 6000.6.4f1 CLI
echo ========================================================

set UNITY_PATH="C:\Program Files\Unity\Hub\Editor\6000.6.4f1\Editor\Unity.exe"
set PROJECT_PATH="%~dp0."

%UNITY_PATH% -batchmode -quit -projectPath %PROJECT_PATH% -executeMethod BuildScript.BuildWindows -logFile "%PROJECT_PATH%\build.log"

if %ERRORLEVEL% EQU 0 (
    echo.
    echo [SUCCESS] Nightbound built successfully!
    echo Executable: %PROJECT_PATH%\Builds\Nightbound.exe
) else (
    echo.
    echo [ERROR] Build failed. Check build.log for details.
)
